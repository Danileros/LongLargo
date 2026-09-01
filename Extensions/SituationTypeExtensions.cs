using Il2Cpp;
using LongLargo.Models;

namespace LongLargo.Extensions;

public static class SituationTypeExtensions
{
    public static FSituationType GetWeathers()
    {
        return FSituationType.WeatherClear
                | FSituationType.WeatherFog
                | FSituationType.WeatherSnow
                | FSituationType.WeatherBlizzard;
    }
    
    public static FSituationType GetTimes()
    {
        return FSituationType.TimeDawn
                | FSituationType.TimeDusk;
    }
    
    public static FSituationType GetExplorations()
    {
        return FSituationType.ExplorationAurora
                | FSituationType.ExplorationDay
                | FSituationType.ExplorationNight;
    }
    
    public static FSituationType GetConditions()
    {
        return FSituationType.ConditionSorrow
                | FSituationType.ConditionSuccess;
    }
    
    public static FSituationType GetDangers()
    {
        return FSituationType.Stalked
               | FSituationType.Timberwolf;
    }

    public static bool HasFlagSafe(this FSituationType situationType, FSituationType flag)
    {
        return flag != FSituationType.Disabled && situationType.HasFlag(flag);
    }
    
    public static bool IsWeather(this FSituationType situationType)
    {
        return SituationTypeExtensions.GetWeathers().HasFlagSafe(situationType);
    }
    
    public static bool IsTime(this FSituationType situationType)
    {
        return SituationTypeExtensions.GetTimes().HasFlagSafe(situationType);
    }
    
    public static bool IsExploration(this FSituationType situationType)
    {
        return SituationTypeExtensions.GetExplorations().HasFlagSafe(situationType);
    }
    
    public static bool IsCondition(this FSituationType situationType)
    {
        return SituationTypeExtensions.GetConditions().HasFlagSafe(situationType);
    }
    
    public static bool IsDanger(this FSituationType situationType)
    {
        return SituationTypeExtensions.GetDangers().HasFlagSafe(situationType);
    }

    public static FSituationType GetExplorationSituation()
    {
        try
        {
            var timeOfDay = GameManager.GetTimeOfDayComponent();
            var aurora = GameManager.GetAuroraManager();
            var situation = aurora.AuroraIsActive()
                ? FSituationType.ExplorationAurora
                : (timeOfDay.IsDay() ? FSituationType.ExplorationDay : FSituationType.ExplorationNight);
            return situation;
        }
        catch
        {
            return FSituationType.Disabled;
        }
    }
}