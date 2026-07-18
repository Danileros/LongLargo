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
}