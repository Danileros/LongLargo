namespace LongLargo.Models;

public enum EExplorationMusicMode
{
    /// <summary>
    /// Never play any exploration music.
    /// </summary>
    Suppress = 0,
    
    /// <summary>
    /// Never play vanilla music.
    /// </summary>
    CustomOnly,
    
    /// <summary>
    /// x0.5.
    /// </summary>
    X05,
    
    /// <summary>
    /// x1.
    /// </summary>
    Default,
    
    /// <summary>
    /// x2.
    /// </summary>
    X2,
    
    /// <summary>
    /// x5.
    /// </summary>
    X5,
    
    /// <summary>
    /// x10.
    /// </summary>
    X10,
    
    /// <summary>
    /// Half of the time.
    /// </summary>
    Balanced,
    
    /// <summary>
    /// Never play custom soundtracks.
    /// </summary>
    Vanilla,
}