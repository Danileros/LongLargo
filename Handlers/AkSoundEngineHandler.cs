namespace LongLargo.Handlers;

public static class AkSoundEngineHandler
{
    /// <summary>
    /// Decides whatever we play custom exploration clip and which exactly.
    /// </summary>
    /// <returns>false if we should suppress original music.</returns>
    public static bool PostEvent(string name)
    {
        return true;
    }
}