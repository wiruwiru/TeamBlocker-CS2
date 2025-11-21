using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace TeamBlocker.Configs;

public class BaseConfigs : BasePluginConfig
{
	[JsonPropertyName("TeamSettings")]
	public TeamSettings TeamSettings { get; set; } = new();

	[JsonPropertyName("SoundSettings")]
	public SoundSettings SoundSettings { get; set; } = new();

	[JsonPropertyName("EnableDebug")]
	public bool EnableDebug { get; set; } = false;
}

public class TeamSettings
{
	[JsonPropertyName("MaxCounterTerrorists")]
	public int MaxCounterTerrorists { get; set; } = 5;

	[JsonPropertyName("MaxTerrorists")]
	public int MaxTerrorists { get; set; } = 5;

	[JsonPropertyName("MoveToSpectatorOnConnect")]
	public bool MoveToSpectatorOnConnect { get; set; } = true;

	[JsonPropertyName("IgnoreDuringWarmup")]
	public bool IgnoreDuringWarmup { get; set; } = true;
}

public class SoundSettings
{
	[JsonPropertyName("SoundFilePath")]
	public string? SoundFilePath { get; set; } = "soundevents/game_sounds_ui.vsndevts";

	[JsonPropertyName("SoundEvent")]
	public string? SoundEvent { get; set; } = "Vote.Failed";
}