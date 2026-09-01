using System.Text.Json.Serialization;

namespace LongLargo.Models;

/// <summary>
/// Situation type for playlist.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FSituationType
{
    Any = -1,
    Disabled = 0,
    ExplorationDay = 1,
    ExplorationNight = 1 << 1,
    ExplorationAurora = 1 << 2,
    WeatherClear = 1 << 3,
    WeatherFog = 1 << 4,
    WeatherSnow = 1 << 5,
    WeatherBlizzard = 1 << 6,
    TimeDusk = 1 << 7,
    TimeDawn = 1 << 8,
    Stalked = 1 << 9,
    Timberwolf = 1 << 10,
    ConditionSuccess = 1 << 11,
    ConditionSorrow = 1 << 12,
}