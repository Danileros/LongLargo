using LongLargo.Models;

namespace LongLargo.Utils;

public static class ScenesHelper
{
    public static bool IsForbidden(string sceneName)
    {
        return sceneName == null || sceneName == "Empty" || sceneName.Contains("Menu") || sceneName.Contains("Boot");
    }
    
    public static bool IsTales(string sceneName)
    {
        return sceneName.Contains("Bunker") || sceneName == "MiningRegionMine";
    }
    
    public static LocationType GetLocationType(string sceneName)
    {
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