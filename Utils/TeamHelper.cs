using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace TeamBlocker.Utils;

public static class TeamHelper
{
    public static int GetCurrentNumPlayers(CsTeam? csTeam = null, bool includeBots = false)
    {
        return Utilities.GetPlayers().Count(player => 
            player?.IsValid == true && 
            (includeBots || (!player.IsBot && !player.IsHLTV)) && 
            (csTeam == null || player.Team == csTeam));
    }

    public static int GetCurrentNumPlayersExcept(CsTeam? csTeam, CCSPlayerController? excludePlayer, bool includeBots = false)
    {
        return Utilities.GetPlayers().Count(player => 
            player?.IsValid == true && 
            player != excludePlayer &&
            (includeBots || (!player.IsBot && !player.IsHLTV)) && 
            (csTeam == null || player.Team == csTeam));
    }
}