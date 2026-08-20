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
    
    public static LocationType GetLocationType(string sceneName)
    {
        if (IsMenu(sceneName) || IsSubscene(sceneName))
        {
            return LocationType.Disabled;
        }
        
        if (sceneName.EndsWith("Region")
            || sceneName.EndsWith("SurvivalZone")) // BlackrockPrisonSurvivalZone is a part of BlackrockRegion for me
        {
            return LocationType.Region;
        }

        if (sceneName.Contains("MineTransitionZone"))
        {
            return LocationType.Mine;
        }

        if (sceneName.Contains("CaveTransitionZone"))
        {
            return LocationType.Cave;
        }

        if (sceneName.Contains("DamTransitionZone"))
        {
            return LocationType.Building;
        }

        if (sceneName.Contains("TransitionZone"))
        {
            return LocationType.TransitionZone;
        }

        if (sceneName.Contains("Cave"))
        {
            return LocationType.Cave;
        }

        if (sceneName.Contains("Mine"))
        {
            return LocationType.Mine;
        }

        return LocationType.Building;
    }
}