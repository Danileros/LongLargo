using System.Text.Json.Serialization;

namespace LongLargo.Models;

/// <summary>
/// Location type for playlist.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FLocationType
{
    Any = -1,
    Disabled = 0,
    Region = 1,
    TransitionZone = 1 << 1,
    Building = 1 << 2,
    Cave = 1 << 3,
    Mine = 1 << 4,
}