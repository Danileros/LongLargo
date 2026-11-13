// using System.Reflection;
// using System.Runtime.CompilerServices;
// using MelonLoader;
//
// namespace LongLargo.Managers;
//
// public class ComplexLoggerProxy<T> where T : MelonBase
// {
//     private readonly Action<FlaggedLoggingLevel, LoggingSubType, string> _log1Delegate;
//     private readonly Action<string, FlaggedLoggingLevel, LoggingSubType, string> _log2Delegate;
//     private readonly Action<string, FlaggedLoggingLevel, string> _log3Delegate;
//     private readonly Action<string, FlaggedLoggingLevel, Exception, string> _log4Delegate;
//     private readonly Action<string, string[], FlaggedLoggingLevel> _log5Delegate;
//     
//
//     public ComplexLoggerProxy()
//     {
//         try
//         {
//             var assembliesList = AppDomain.CurrentDomain.GetAssemblies();
//             var complexLoggerAssembly = assembliesList
//                 .FirstOrDefault(assembly => assembly.GetName().Name == "ComplexLogger");
//             var proxifiedType = complexLoggerAssembly?.GetType("ComplexLogger.ComplexLogger`1");
//             if (proxifiedType != null)
//             {
//                 var type = proxifiedType.MakeGenericType(typeof(T));
//                 var loggingLevelType = complexLoggerAssembly.GetType("ComplexLogger.FlaggedLoggingLevel");
//                 var loggingSubType = complexLoggerAssembly.GetType("ComplexLogger.LoggingSubType");
//                 var instance = Activator.CreateInstance(type);
//                 
//                 var log1
//                     = type.GetMethod(
//                         "Log",
//                         BindingFlags.Instance | BindingFlags.Public,
//                         new [] { loggingLevelType, loggingSubType, typeof(string)});
//                 
//                 if (log1 != null)
//                 {
//                     _log1Delegate = (Action<FlaggedLoggingLevel, LoggingSubType, string>)
//                         Delegate.CreateDelegate(typeof(Action<FlaggedLoggingLevel, LoggingSubType, string>), instance, log1);
//                 }
//                 
//                 var log2
//                     = type.GetMethod(
//                         "Log",
//                         BindingFlags.Instance | BindingFlags.Public,
//                         new [] { typeof(string), loggingLevelType, loggingSubType, typeof(string)});
//                 
//                 if (log2 != null)
//                 {
//                     _log2Delegate = (Action<string, FlaggedLoggingLevel, LoggingSubType, string>)
//                         Delegate.CreateDelegate(typeof(Action<string, FlaggedLoggingLevel, LoggingSubType, string>), instance, log2);
//                 }
//                 
//                 var log3
//                     = type.GetMethod(
//                         "Log",
//                         BindingFlags.Instance | BindingFlags.Public,
//                         new [] { typeof(string), loggingLevelType, typeof(string)});
//                 
//                 if (log3 != null)
//                 {
//                     _log3Delegate = (Action<string, FlaggedLoggingLevel, string>)
//                         Delegate.CreateDelegate(typeof(Action<string, FlaggedLoggingLevel, string>), instance, log3);
//                 }
//                 
//                 var log4
//                     = type.GetMethod(
//                         "Log",
//                         BindingFlags.Instance | BindingFlags.Public,
//                         new [] { typeof(string), loggingLevelType, typeof(Exception), typeof(string)});
//                 
//                 if (log4 != null)
//                 {
//                     _log4Delegate = (Action<string, FlaggedLoggingLevel, Exception, string>)
//                         Delegate.CreateDelegate(typeof(Action<string, FlaggedLoggingLevel, Exception, string>), instance, log4);
//                 }
//             }
//         }
//         catch (Exception)
//         {
//             // ignored
//         }
//     }
//     
//     /// <summary>
//     /// For when you need to log a separator
//     /// </summary>
//     /// <param name="level"></param>
//     /// <param name="LogSubType"></param>
//     /// <param name="memberName">This should never be filled by your log call. This is just used to add method info to your log</param>
//     public void Log(FlaggedLoggingLevel level, LoggingSubType LogSubType, [CallerMemberName] string memberName = "")
//     {
//         if (_log1Delegate != null)
//         {
//             _log1Delegate(level, LogSubType, memberName);
//         }
//     }
//
//     /// <summary>
//     /// for when you need to log a header
//     /// </summary>
//     /// <param name="message"></param>
//     /// <param name="level"></param>
//     /// <param name="LogSubType"></param>
//     /// <param name="memberName">This should never be filled by your log call. This is just used to add method info to your log</param>
//     public void Log(string message, FlaggedLoggingLevel level, LoggingSubType LogSubType, [CallerMemberName] string memberName = "")
//     {
//         if (_log2Delegate != null)
//         {
//             _log2Delegate(message, level, LogSubType, memberName);
//         }
//     }
//
//     /// <summary>
//     /// 
//     /// </summary>
//     /// <param name="message"></param>
//     /// <param name="level"></param>
//     /// <param name="memberName">This should never be filled by your log call. This is just used to add method info to your log</param>
//     public void Log(string message, FlaggedLoggingLevel level, [CallerMemberName] string memberName = "")
//     {
//         if (_log3Delegate != null)
//         {
//             _log3Delegate(message, level, memberName);
//         }
//     }
//
//     /// <summary>
//     /// 
//     /// </summary>
//     /// <param name="message"></param>
//     /// <param name="level"></param>
//     /// <param name="exception"></param>
//     /// <param name="memberName">This should never be filled by your log call. This is just used to add method info to your log</param>
//     public void Log(string message, FlaggedLoggingLevel level, System.Exception exception, [CallerMemberName] string memberName = "")
//     {
//         if (_log4Delegate != null)
//         {
//             _log4Delegate(message, level, exception, memberName);
//         }
//         else
//         {
//             _logInternal(message, level);
//         }
//     }
//
//     /// <summary>
//     /// Allows you to log an entire block, using <see cref="WriteLogBlock(string, string[], System.Drawing.Color)"/>
//     /// </summary>
//     /// <param name="Title">What you want the block to be titled</param>
//     /// <param name="Lines">Each line that you want in that block</param>
//     /// <param name="Color">The color of ALL lines in that block (use <see cref="Log(string, string[], List{System.Drawing.Color}, FlaggedLoggingLevel)"/> to be able to color the other lines individually)</param>
//     /// <param name="Level">What level you want this block to write at</param>
//     public void Log(string Title, string[] Lines, System.Drawing.Color Color, FlaggedLoggingLevel Level)
//     {
//         
//     }
//
//     /// <summary>
//     /// Allows you to log an entire block, using <see cref="WriteLogBlock(string, string[], System.Drawing.Color)"/>
//     /// </summary>
//     /// <param name="Title">What you want the block to be titled</param>
//     /// <param name="Lines">Each line that you want in that block</param>
//     /// <param name="Color">The color of ALL lines in that block (use <see cref="Log(string, string[], System.Drawing.Color, FlaggedLoggingLevel)"/> to be able to color the other lines individually)</param>
//     /// <param name="Level">What level you want this block to write at</param>
//     public void Log(string Title, string[] Lines, List<System.Drawing.Color> Color, FlaggedLoggingLevel Level)
//     {
//         
//     }
//
//     /// <summary>
//     /// Print a log if the current level matches the level given.
//     /// </summary>
//     /// <param name="message">Formatted string to use in this log</param>
//     /// <param name="level">The level of this message (NOT the existing the level)</param>
//     /// <param name="exception">The exception, if applicable, to display</param>
//     /// <param name="LogSubType">Used to write separators only when the logging level matches the current flags</param>
//     /// <param name="memberName">This should never be filled by your log call. This is just used to add method info to your log</param>
//     public void Log(string message, FlaggedLoggingLevel level, LoggingSubType? LogSubType, System.Exception exception, [CallerMemberName] string memberName = "")
//     {
//         
//     }
//
//     public void Log(FlaggedLoggingLevel level, LoggingSubType LogSubType, params object[] parameters)
//     {
//         
//     }
//     
//     public void Log(string message, FlaggedLoggingLevel level, LoggingSubType LogSubType, params object[] parameters)
//     {
//         
//     }
//     
//     public void Log(string message, FlaggedLoggingLevel level, params object[] parameters)
//     {
//         
//     }
//     
//     public void Log(string message, FlaggedLoggingLevel level, System.Exception exception, params object[] parameters)
//     {
//         
//     }
//     
//     public void Log(string message, FlaggedLoggingLevel level, LoggingSubType? LogSubType, System.Exception exception, params object[] parameters)
//     {
//         
//     }
//
//     /// <summary>
//     /// The base log method
//     /// </summary>
//     /// <param name="message">The formated string to add as the message</param>
//     /// <param name="parameters">Any additional params, note that this must be either a single item or an array <c>Write("", [object, object])</c></param>
//     public void Write(string message, params object[] parameters)
//     {
//         
//     }
//
//     private void _logInternal(string message, FlaggedLoggingLevel level)
//     {
//         switch (level)
//         {
//             case FlaggedLoggingLevel.None:
//             case FlaggedLoggingLevel.Always:
//                 MelonLoader.MelonLogger.Msg(message);
//                 break;
//             case FlaggedLoggingLevel.Error:
//                 MelonLoader.MelonLogger.Error(message);
//                 break;
//             case FlaggedLoggingLevel.Trace:
//             case FlaggedLoggingLevel.Debug:
//             case FlaggedLoggingLevel.Verbose:
//             case FlaggedLoggingLevel.Warning:
//                 break;
//             case FlaggedLoggingLevel.Critical:
//                 break;
//             case FlaggedLoggingLevel.Exception:
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(level), level, null);
//         }
//     }
// }
//
// /// <summary></summary>
// [System.Flags]
// public enum FlaggedLoggingLevel
// {
//     /// <summary></summary>
//     [Obsolete("Use Always instead")]
//     None			= 0b_0000_0000,
//     /// <summary>Use this for debug messages that dont matter 99% of the time</summary>
//     Trace			= 0b_0000_0001,
//     /// <summary>Your go to level for pretty much everything. If the method is being called on update, please use Verbose instead!</summary>
//     Debug			= 0b_0000_0010,
//     /// <summary>For those errors that you can use but dont matter much in general debugging. Also required to use this for any and all update methods!</summary>
//     Verbose			= 0b_0000_0100,
//     /// <summary>Used for when something happens that wont break things but is still something that shouldnt happen</summary>
//     Warning			= 0b_0000_1000,
//     /// <summary>Like warning, this is used for when something happens that shouldnt, but this one is for when it does break things</summary>
//     Error			= 0b_0001_0000,
//     /// <summary>This is for when things really break</summary>
//     Critical		= 0b_0010_0000,
//     /// <summary>This is intended to be used within exception catches, ensure to add the exception instance to your log call</summary>
//     Exception		= 0b_0100_0000,
//     /// <summary>For when you want to log the message at all times. Please dont use this for startup messages or anything that will print more than once or twice. Useful for console command printed logs</summary>
//     Always			= 0b_1000_0000
// }
//
// /// <summary>
// /// Simple enum to handle the different types of logging
// /// </summary>
// public enum LoggingSubType
// {
//     /// <summary>
//     /// General use. This is used by default
//     /// </summary>
//     Normal,
//     /// <summary>
//     /// To print a separator
//     /// </summary>
//     Separator,
//     /// <summary>
//     /// To print a header
//     /// </summary>
//     IntraSeparator,
//     /// <summary>
//     /// Print to the Unity Console
//     /// </summary>
//     uConsole,
//     /// <summary>
//     /// Print a block to the log NOT IMPLEMENTED AT THIS TIME (Writing this is a bit complex), see the class for the current syntax
//     /// </summary>
//     Block
// }