using Il2Cpp;
using LongLargo.Model;

namespace LongLargo.Extensions;

public static class SituationTypeExtensions
{
    public static bool IsWeather(this SituationType situationType)
    {
        return (SituationType.WeatherClear
                | SituationType.WeatherFog
                | SituationType.WeatherSnow
                | SituationType.WeatherBlizzard).HasFlag(situationType);
    }
    
    public static bool IsTime(this SituationType situationType)
    {
        return (SituationType.TimeDawn
                | SituationType.TimeDusk).HasFlag(situationType);
    }
    
    public static bool IsExploration(this SituationType situationType)
    {
        return (SituationType.ExplorationAurora
                | SituationType.ExplorationDay
                | SituationType.ExplorationNight).HasFlag(situationType);
    }
    
    public static bool IsCondition(this SituationType situationType)
    {
        return (SituationType.ConditionSorrow
                | SituationType.ConditionSuccess).HasFlag(situationType);
    }
    

    public static SituationType GetExplorationSituation()
    {
        var timeOfDay = GameManager.GetTimeOfDayComponent();
        var aurora = GameManager.GetAuroraManager();
        var situation = aurora.AuroraIsActive() ? SituationType.ExplorationAurora : (timeOfDay.IsDay() ? SituationType.ExplorationDay : SituationType.ExplorationNight);
        return situation;
    }
}