using System.Reflection;
using MelonLoader;

namespace LongLargo.Utils;

/// <summary>
/// LL Logging proxy.
/// </summary>
public class LoggerProxy<T> where T : MelonBase
{
    private readonly Action<string, FlaggedLoggingLevel, string> _logDelegate;
    
    public LoggerProxy()
    {
        var assembliesList = AppDomain.CurrentDomain.GetAssemblies();
        var complexLoggerAssembly = assembliesList
            .FirstOrDefault(assembly => assembly.GetName().Name == "ComplexLogger");
        var proxifiedType = complexLoggerAssembly?.GetType("ComplexLogger.ComplexLogger`1");
        if (proxifiedType != null)
        {
            var type = proxifiedType.MakeGenericType(typeof(T));
            var loggingLevelType = complexLoggerAssembly.GetType("ComplexLogger.FlaggedLoggingLevel");
            var loggingSubType = complexLoggerAssembly.GetType("ComplexLogger.LoggingSubType");
            var instance = Activator.CreateInstance(type);
            
            var log
                = type.GetMethod(
                    "Log",
                    BindingFlags.Instance | BindingFlags.Public,
                    new [] { typeof(string), loggingLevelType, typeof(string)});
                
            if (log != null)
            {
                _logDelegate = (Action<string, FlaggedLoggingLevel, string>)
                    Delegate.CreateDelegate(typeof(Action<string, FlaggedLoggingLevel, string>), instance, log);
            }
        }
    }
    
    public void Log(string message)
    {
        if (_logDelegate != null)
        {
            _logDelegate(message, FlaggedLoggingLevel.Always, string.Empty);
        }
        else
        {
            MelonLoader.MelonLogger.Msg(message);
        }
    }

    public void Log(string txt, params object[] args)
    {
        var message = string.Format(txt, args);
        Log(message);
    }
    
    public void Debug(string message)
    {
        if (_logDelegate != null)
        {
            _logDelegate(message, FlaggedLoggingLevel.Debug, string.Empty);
        }
    }

    public void Error(string message)
    {
        if (_logDelegate != null)
        {
            _logDelegate(message, FlaggedLoggingLevel.Error, string.Empty);
        }
        else
        {
            MelonLoader.MelonLogger.Error(message);
        }
    }
    
    /// <summary></summary>
    [System.Flags]
    public enum FlaggedLoggingLevel
    {
        /// <summary></summary>
        [Obsolete("Use Always instead")]
        None			= 0b_0000_0000,
        /// <summary>Use this for debug messages that dont matter 99% of the time</summary>
        Trace			= 0b_0000_0001,
        /// <summary>Your go to level for pretty much everything. If the method is being called on update, please use Verbose instead!</summary>
        Debug			= 0b_0000_0010,
        /// <summary>For those errors that you can use but dont matter much in general debugging. Also required to use this for any and all update methods!</summary>
        Verbose			= 0b_0000_0100,
        /// <summary>Used for when something happens that wont break things but is still something that shouldnt happen</summary>
        Warning			= 0b_0000_1000,
        /// <summary>Like warning, this is used for when something happens that shouldnt, but this one is for when it does break things</summary>
        Error			= 0b_0001_0000,
        /// <summary>This is for when things really break</summary>
        Critical		= 0b_0010_0000,
        /// <summary>This is intended to be used within exception catches, ensure to add the exception instance to your log call</summary>
        Exception		= 0b_0100_0000,
        /// <summary>For when you want to log the message at all times. Please dont use this for startup messages or anything that will print more than once or twice. Useful for console command printed logs</summary>
        Always			= 0b_1000_0000
    }
}