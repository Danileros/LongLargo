using Il2Cpp;
using LongLargo.Model;

namespace LongLargo.Extensions;

public static class SituationTypeExtensions
{
    public static SituationType GetWeathers()
    {
        return SituationType.WeatherClear
                | SituationType.WeatherFog
                | SituationType.WeatherSnow
                | SituationType.WeatherBlizzard;
    }
    
    public static SituationType GetTimes()
    {
        return SituationType.TimeDawn
                | SituationType.TimeDusk;
    }
    
    public static SituationType GetExplorations()
    {
        return SituationType.ExplorationAurora
                | SituationType.ExplorationDay
                | SituationType.ExplorationNight;
    }
    
    public static SituationType GetConditions()
    {
        return SituationType.ConditionSorrow
                | SituationType.ConditionSuccess;
    }
    
    public static SituationType GetDangers()
    {
        return SituationType.Stalked
               | SituationType.Timberwolf;
    }
    
    public static bool IsWeather(this SituationType situationType)
    {
        return SituationTypeExtensions.GetWeathers().HasFlag(situationType);
    }
    
    public static bool IsTime(this SituationType situationType)
    {
        return SituationTypeExtensions.GetTimes().HasFlag(situationType);
    }
    
    public static bool IsExploration(this SituationType situationType)
    {
        return SituationTypeExtensions.GetExplorations().HasFlag(situationType);
    }
    
    public static bool IsCondition(this SituationType situationType)
    {
        return SituationTypeExtensions.GetConditions().HasFlag(situationType);
    }
    
    public static bool IsDanger(this SituationType situationType)
    {
        return SituationTypeExtensions.GetDangers().HasFlag(situationType);
    }

    public static SituationType GetExplorationSituation()
    {
        var timeOfDay = GameManager.GetTimeOfDayComponent();
        var aurora = GameManager.GetAuroraManager();
        var situation = aurora.AuroraIsActive() ? SituationType.ExplorationAurora : (timeOfDay.IsDay() ? SituationType.ExplorationDay : SituationType.ExplorationNight);
        return situation;
    }
}