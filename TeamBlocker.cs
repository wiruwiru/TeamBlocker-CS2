using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;

using TeamBlocker.Configs;
using TeamBlocker.Utils;

namespace TeamBlocker;

[MinimumApiVersion(346)]
public class TeamBlocker : BasePlugin, IPluginConfig<BaseConfigs>
{
	public override string ModuleName => "TeamBlocker";
	public override string ModuleVersion => "1.0.0";
	public override string ModuleAuthor => "luca.uy";
	public override string ModuleDescription => "Restricts how many players can join a team.";

	public required BaseConfigs Config { get; set; }
	private CCSGameRules? _gameRules;

	public void OnConfigParsed(BaseConfigs config)
	{
		Config = config;
		Utils.Logger.Config = config;
	}

	public override void Load(bool hotReload)
	{
		if (hotReload)
		{
			Utils.Logger.LogInfo("Plugin", "Reloading plugin...");
		}

		AddCommandListener("jointeam", JoinTeamListener, HookMode.Pre);
		RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
		RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam, HookMode.Pre);
		RegisterEventHandler<EventRoundStart>(OnRoundStart, HookMode.Post);
	}

	public override void OnAllPluginsLoaded(bool hotReload)
	{
		RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
	}

	private void OnServerPrecacheResources(ResourceManifest manifest)
	{
		if (!string.IsNullOrWhiteSpace(Config.SoundSettings.SoundFilePath))
		{
			manifest.AddResource(Config.SoundSettings.SoundFilePath);
		}
	}

	private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
	{
		_gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;
		return HookResult.Continue;
	}

	private bool IsWarmup()
	{
		if (_gameRules == null)
		{
			_gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;
		}
		return _gameRules?.WarmupPeriod ?? false;
	}

	private bool ShouldIgnoreLimits()
	{
		if (!Config.TeamSettings.IgnoreDuringWarmup)
			return false;

		bool isWarmup = IsWarmup();
		Utils.Logger.LogDebug("WarmupCheck", $"IsWarmup: {isWarmup}, IgnoreLimits: {Config.TeamSettings.IgnoreDuringWarmup}");
		return isWarmup;
	}

	public HookResult JoinTeamListener(CCSPlayerController? player, CommandInfo info)
	{
		if (player is null || !player.IsValid)
		{
			return HookResult.Continue;
		}

		if (info.ArgCount < 2)
		{
			return HookResult.Continue;
		}

		string targetTeamArg = info.GetArg(1);
		if (targetTeamArg == "1")
		{
			Utils.Logger.LogDebug("JoinTeamListener", "Spectator requested - changing team");
			Server.NextFrame(() =>
			{
				if (player.IsValid)
				{
					player.ChangeTeam(CsTeam.Spectator);
				}
			});
			return HookResult.Handled;
		}

		if (targetTeamArg == "0")
		{
			Utils.Logger.LogDebug("JoinTeamListener", "Auto-select team requested");
			if (!ShouldIgnoreLimits())
			{
				Utils.Logger.LogDebug("JoinTeamListener", $"Auto-select BLOCKED for {player.PlayerName} - not in warmup");
				
				Server.NextFrame(() =>
				{
					if (player.IsValid)
					{
						player.PrintToChat($" {Localizer["prefix"]} {Localizer["autoselect.blocked"] ?? "Auto-select is disabled. Please choose a team manually."}");
					}
				});
				
				return HookResult.Handled;
			}
			
			return HookResult.Continue;
		}

		if (targetTeamArg != "2" && targetTeamArg != "3")
		{
			Utils.Logger.LogDebug("JoinTeamListener", $"Unknown team arg '{targetTeamArg}', allowing");
			return HookResult.Continue;
		}

		CsTeam targetTeam = targetTeamArg == "2" ? CsTeam.Terrorist : CsTeam.CounterTerrorist;
		string teamName = targetTeamArg == "2" ? "Terrorist" : "Counter-Terrorist";

		if (player.Team == targetTeam)
		{
			Utils.Logger.LogDebug("JoinTeamListener", $"Player already in target team {targetTeam}, allowing");
			return HookResult.Continue;
		}

		if (ShouldIgnoreLimits())
		{
			Utils.Logger.LogDebug("TeamJoin", $"Warmup active - ignoring team limits for {player.PlayerName}, allowing normal team join");
			return HookResult.Continue;
		}

		int ctCount = TeamHelper.GetCurrentNumPlayersExcept(CsTeam.CounterTerrorist, player);
		int tCount = TeamHelper.GetCurrentNumPlayersExcept(CsTeam.Terrorist, player);

		int ctMax = Config.TeamSettings.MaxCounterTerrorists;
		int tMax = Config.TeamSettings.MaxTerrorists;

		Utils.Logger.LogDebug("TeamJoin", $"Player {player.PlayerName} attempting to join {teamName}");
		Utils.Logger.LogDebug("TeamJoin", $"Current counts: CT={ctCount}/{ctMax}, T={tCount}/{tMax}");

		bool isBlocked = false;
		int maxCount = 0;

		if (targetTeamArg == "3" && ctCount >= ctMax)
		{
			isBlocked = true;
			maxCount = ctMax;
		}
		else if (targetTeamArg == "2" && tCount >= tMax)
		{
			isBlocked = true;
			maxCount = tMax;
		}

		if (isBlocked)
		{
			Utils.Logger.LogDebug("TeamJoin", $"Player {player.PlayerName} BLOCKED from joining {teamName} team");

			Server.NextFrame(() =>
			{
				if (player.IsValid)
				{
					player.PrintToChat($" {Localizer["prefix"]} {Localizer["team.full", teamName, maxCount]}");
				}
			});

			if (player.Team is CsTeam.None)
			{
				AddTimer(3.0f, () =>
				{
					if (player.IsValid && !ShouldIgnoreLimits())
					{
						player.ChangeTeam(CsTeam.Spectator);
						Utils.Logger.LogDebug("TeamJoin", $"Moved {player.PlayerName} to spectator after 3s delay");
					}
				});
			}

			Server.NextFrame(() =>
			{
				try
				{
					if (player.IsValid && !string.IsNullOrWhiteSpace(Config.SoundSettings.SoundEvent))
					{
						var filter = new RecipientFilter(player);
						player?.EmitSound(Config.SoundSettings.SoundEvent, volume: 1f, recipients: filter);
					}
				}
				catch (Exception ex)
				{
					Utils.Logger.LogError("Sound", $"Error playing sound to player {player.PlayerName}: {ex.Message}");
				}
			});

			return HookResult.Handled;
		}

		Utils.Logger.LogDebug("TeamJoin", $"Player {player.PlayerName} ALLOWED to join {teamName} team - executing change");

		Server.NextFrame(() =>
		{
			if (player.IsValid)
			{
				player.ChangeTeam(targetTeam);
			}
		});

		return HookResult.Handled;
	}

	private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;

		if (player is null || !player.IsValid || player.IsBot || player.IsHLTV)
			return HookResult.Continue;

		if (!Config.TeamSettings.MoveToSpectatorOnConnect)
			return HookResult.Continue;

		if (ShouldIgnoreLimits())
		{
			Utils.Logger.LogDebug("PlayerConnect", $"Warmup active - not moving {player.PlayerName} to spectator");
			return HookResult.Continue;
		}

		AddTimer(3.0f, () =>
		{
			if (player.IsValid && !player.IsBot && !player.IsHLTV)
			{
				if (!ShouldIgnoreLimits())
				{
					player.ChangeTeam(CsTeam.Spectator);
					Utils.Logger.LogDebug("PlayerConnect", $"Moved {player.PlayerName} to spectator after 3s delay");
				}
			}
		});

		return HookResult.Continue;
	}

	private HookResult OnPlayerTeam(EventPlayerTeam @event, GameEventInfo info)
	{
		info.DontBroadcast = true;
		return HookResult.Changed;
	}

	public override void Unload(bool hotReload)
	{
		RemoveCommandListener("jointeam", JoinTeamListener, HookMode.Pre);
		Utils.Logger.LogDebug("Plugin", "Plugin unloaded, clearing data...");
	}
}