using LongLargo.Models;

namespace LongLargo.Interfaces;

/// <summary>
/// Loads and provides playlist and tracks.
/// </summary>
public interface IPlaylistManager
{
    /// <summary>
    /// All sountracks
    /// </summary>
    IReadOnlyCollection<SoundtrackInfo> Soundtracks { get; }
    
    /// <summary>
    /// 4 minutes silence track.
    /// </summary>
    SoundtrackInfo LongSilence { get; }
    
    /// <summary>
    /// 1 minute silence track.
    /// </summary>
    SoundtrackInfo ShortSilence { get; }
    
    /// <summary>
    /// Gets exploration clip by name.
    /// </summary>
    /// <returns>(SoundtrackInfo, playVanilla)</returns>
    SoundtrackInfo GetSoundtrackByName(string name);

    /// <summary>
    /// Gets random exploration clip.
    /// </summary>
    /// <returns>(SoundtrackInfo, playVanilla)</returns>
    (SoundtrackInfo, bool) GetExplorationSoundtrack(FSituationType situation, bool excludeVanillaMusic = false);

    /// <summary>
    /// Gets random situation soundtrack.
    /// </summary>
    /// <returns>(SoundtrackInfo, playVanilla)</returns>
    (SoundtrackInfo, bool) GetSituationSoundtrack(FSituationType situation, bool excludeVanillaMusic = false);
}