using System.Collections.Generic;

namespace LongLargo.Model;

/// <summary>
/// Playlist information (used for serialization, updates on game start automatically).
/// </summary>
public class PlaylistInfo
{
    public List<SoundtrackInfo> SoundtrackInfos { get; set; } = new List<SoundtrackInfo>();
}