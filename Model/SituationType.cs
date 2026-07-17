using System;
using System.Text.Json.Serialization;

namespace LongLargo.Model;

/// <summary>
/// Situation type for playlist.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SituationType
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
    Success = 1 << 11,
    Sorrow = 1 << 12,
    
    SilenceShort = 1 << 20,
    SilenceLong = 1 << 21,
}