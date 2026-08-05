using System;
using System.Text.Json.Serialization;
using AudioMgr;
using LongLargo.Utils;
using UnityEngine;

namespace LongLargo.Model;

/// <summary>
/// Information about soundtrack and when it should play.
/// </summary>
public class SoundtrackInfo
{
    /// <summary>
    /// AssetBundle it belongs to.
    /// </summary>
    [JsonIgnore]
    public AssetBundle AssetBundle { get; set; } = null;
    
    /// <summary>
    /// Clip.
    /// </summary>
    [JsonIgnore]
    public Clip Clip { get; set; } = null;
    
    /// <summary>
    /// Track name (must be not empty and unique).
    /// </summary>
    public string TrackName { get; set; } = string.Empty;
    
    /// <summary>
    /// Chance to play (default is 100).
    /// </summary>
    public uint Chance { get; set; } = 100;

    /// <summary>
    /// Play only when specific conditions are meet (like windy weather).
    /// If Any, plays anytime.
    /// </summary>
    [JsonConverter(typeof(SafeJsonEnumConverterFactory))]
    public SituationType SituationsRestrictsTo { get; set; } = SituationType.ExplorationDay | SituationType.ExplorationNight;
    
    /// <summary>
    /// Play only at specific location type (outdoors, cave, etc).
    /// If Any, plays at any location type.
    /// </summary>
    [JsonConverter(typeof(SafeJsonEnumConverterFactory))]
    public LocationType LocationsTypeRestrictTo { get; set; } = LocationType.Region | LocationType.TransitionZone;

    /// <summary>
    /// Play only at specific locations (like TPV to The Pleasant Valley).
    /// If empty, plays at any location.
    /// </summary>
    public string[] LocationRestrictTo { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Only for Timberwolf combat loop.
    /// </summary>
    public string StopTrackName { get; set; } = null;
}