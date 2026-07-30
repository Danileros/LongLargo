namespace LongLargo.Helpers;

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
}