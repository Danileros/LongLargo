using Il2Cpp;
using LongLargo.Models;

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

    public static bool HasFlagSafe(this SituationType situationType, SituationType flag)
    {
        return flag != SituationType.Disabled && situationType.HasFlag(flag);
    }
    
    public static bool IsWeather(this SituationType situationType)
    {
        return SituationTypeExtensions.GetWeathers().HasFlagSafe(situationType);
    }
    
    public static bool IsTime(this SituationType situationType)
    {
        return SituationTypeExtensions.GetTimes().HasFlagSafe(situationType);
    }
    
    public static bool IsExploration(this SituationType situationType)
    {
        return SituationTypeExtensions.GetExplorations().HasFlagSafe(situationType);
    }
    
    public static bool IsCondition(this SituationType situationType)
    {
        return SituationTypeExtensions.GetConditions().HasFlagSafe(situationType);
    }
    
    public static bool IsDanger(this SituationType situationType)
    {
        return SituationTypeExtensions.GetDangers().HasFlagSafe(situationType);
    }

    public static SituationType GetExplorationSituation()
    {
        try
        {
            var timeOfDay = GameManager.GetTimeOfDayComponent();
            var aurora = GameManager.GetAuroraManager();
            var situation = aurora.AuroraIsActive()
                ? SituationType.ExplorationAurora
                : (timeOfDay.IsDay() ? SituationType.ExplorationDay : SituationType.ExplorationNight);
            return situation;
        }
        catch
        {
            return SituationType.Disabled;
        }
    }
}