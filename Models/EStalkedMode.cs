namespace LongLargo.Models;

public enum EStalkedMode
{
    /// <summary>
    /// Never play any stalked music.
    /// </summary>
    Suppress = 0,
    
    /// <summary>
    /// Play wintermute stalked music.
    /// </summary>
    Wintermute,
    
    /// <summary>
    /// Play both vanilla and custom.
    /// </summary>
    Default,
    
    /// <summary>
    /// Never play custom soundtracks.
    /// </summary>
    Vanilla,
}