namespace LongLargo.Handlers;

/// <summary>
/// LL Logging proxy.
/// </summary>
public static class LLogger
{
    public static void Log(string message)
    {
        MelonLoader.MelonLogger.Msg(message);
    }

    public static void Log(string txt, params object[] args)
    {
        MelonLoader.MelonLogger.Msg(txt, args);
    }
    
    public static void Debug(string message)
    {
        if (LongLargoMain.DebugMode)
        {
            MelonLoader.MelonLogger.Msg(message);
        }
    }

    public static void Error(string message)
    {
        MelonLoader.MelonLogger.Error(message);
    }
}