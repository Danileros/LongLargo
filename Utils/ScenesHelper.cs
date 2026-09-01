using LongLargo.Models;

namespace LongLargo.Utils;

public static class ScenesHelper
{
    public static bool IsMenu(string sceneName)
    {
        return sceneName == null /*|| sceneName == "Empty"*/ || sceneName.Contains("Menu") || sceneName.Contains("Boot");
    }
    
    public static bool IsTales(string sceneName)
    {
        return sceneName.Contains("Bunker") || sceneName == "MiningRegionMine";
    }
    
    public static bool IsSubscene(string sceneName)
    {
        return sceneName == null || sceneName.Contains("_") || sceneName == "Empty";
    }
    
    public static bool IsExplorationBuilding(string sceneName)
    {
        return sceneName == "BlackrockSteamTunnelsASurvival"
            || sceneName == "Dam"
            || sceneName == "DamTransitionZone";
    }
    
    public static FLocationType GetLocationType(string sceneName)
    {
        if (IsMenu(sceneName) || IsSubscene(sceneName))
        {
            return FLocationType.Disabled;
        }
        
        if (sceneName.EndsWith("Region")
            || sceneName.EndsWith("SurvivalZone")) // BlackrockPrisonSurvivalZone is a part of BlackrockRegion for me
        {
            return FLocationType.Region;
        }

        if (sceneName.Contains("MineTransitionZone"))
        {
            return FLocationType.Mine;
        }

        if (sceneName.Contains("CaveTransitionZone"))
        {
            return FLocationType.Cave;
        }

        if (sceneName.Contains("DamTransitionZone"))
        {
            return FLocationType.Building;
        }

        if (sceneName.Contains("TransitionZone"))
        {
            return FLocationType.TransitionZone;
        }

        if (sceneName.Contains("Cave"))
        {
            return FLocationType.Cave;
        }

        if (sceneName.Contains("Mine"))
        {
            return FLocationType.Mine;
        }

        return FLocationType.Building;
    }
}