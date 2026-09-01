namespace LongLargo.Models;

public enum EWeatherStingerMode
{
    /// <summary>
    /// Never play any weather stinger.
    /// </summary>
    Suppress = 0,
    
    /// <summary>
    /// Never play vanilla stinger.
    /// </summary>
    CustomOnly,
    
    /// <summary>
    /// x1.
    /// </summary>
    Default,
    
    /// <summary>
    /// Half of the time.
    /// </summary>
    Balanced,
    
    /// <summary>
    /// Never play custom soundtracks.
    /// </summary>
    Vanilla,
}