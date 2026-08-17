using System.Reflection;

namespace LongLargo.Managers;

/// <summary>
/// Optional dependency proxy for DebugOutput.DebugManagerProxy. Works if DebugOutput mod is loaded; does nothing if not.
/// </summary>
public class DebugManagerProxy
{
    /// <inheritdoc cref="DebugManagerProxy.RegisterDebugCommandDelegate" />
    public readonly RegisterDebugCommandDelegate RegisterDebugCommand;
    
    /// <inheritdoc cref="DebugManagerProxy.UnregisterDebugCommandDelegate" />
    public readonly UnregisterDebugCommandDelegate UnregisterDebugCommand;

    /// <inheritdoc cref="DebugManagerProxy.GetDebugEnabledDelegate" />
    public readonly GetDebugEnabledDelegate GetDebugEnabled;
    
    /// <inheritdoc cref="DebugManagerProxy.SetDebugEnabledDelegate" />
    public readonly SetDebugEnabledDelegate SetDebugEnabled;
    
    public bool IsActive { get; private set; }

    /// <summary>
    /// Generates proxy delegates if and only if DebugOutput.dll is loaded.
    /// We assume, that if DebugOutput.dll is not loaded - player is not a developer and does not needs debug.
    /// </summary>
    public DebugManagerProxy()
    {
        RegisterDebugCommand = (commandName, func) => { };
        UnregisterDebugCommand = (commandName) => { };
        GetDebugEnabled = (commandName) => false;
        SetDebugEnabled = (commandName, newStatus) => { };
        IsActive = false;
        
        try
        {
            var assembliesList = AppDomain.CurrentDomain.GetAssemblies();
            var debugManagerType = assembliesList
                .FirstOrDefault(assembly => assembly.GetName().Name == "DebugOutput")
                ?.GetType("DebugOutput.DebugManager");
            if (debugManagerType != null)
            {
                var registerDebugCommand
                    = debugManagerType.GetMethod(
                        "RegisterDebugCommand",
                        BindingFlags.Static | BindingFlags.Public);
                var unregisterDebugCommand
                    = debugManagerType.GetMethod(
                        "UnregisterDebugCommand",
                        BindingFlags.Static | BindingFlags.Public);
                var getDebugEnabled
                    = debugManagerType.GetMethod(
                        "GetDebugEnabled", 
                        BindingFlags.Static | BindingFlags.Public);
                var setDebugEnabled
                    = debugManagerType.GetMethod(
                        "SetDebugEnabled", 
                        BindingFlags.Static | BindingFlags.Public);
                
                // CreateDelegate gets rid of slow reflection and optimizes performance
                var registerDebugCommandDelegate = (Action<string, Func<string>>)
                    Delegate.CreateDelegate(typeof(Action<string, Func<string>>), registerDebugCommand);
                var unregisterDebugCommandDelegate = (Action<string>)
                    Delegate.CreateDelegate(typeof(Action<string>), unregisterDebugCommand);
                var getDebugEnabledDelegate = (Func<string, bool>)
                    Delegate.CreateDelegate(typeof(Func<string, bool>), getDebugEnabled);
                var setDebugEnabledDelegate = (Action<string, bool>)
                    Delegate.CreateDelegate(typeof(Action<string, bool>), setDebugEnabled);
                
                // We want proxy methods to have full documentation
                GetDebugEnabled = new GetDebugEnabledDelegate(getDebugEnabledDelegate);
                UnregisterDebugCommand = new UnregisterDebugCommandDelegate(unregisterDebugCommandDelegate);
                SetDebugEnabled = new SetDebugEnabledDelegate(setDebugEnabledDelegate);
                RegisterDebugCommand = new RegisterDebugCommandDelegate(registerDebugCommandDelegate);
                IsActive = true;
                
                MelonLoader.MelonLogger.Msg("DebugManager Proxy compiled successfully");
            }
            else
            {
                MelonLoader.MelonLogger.Msg("DebugManager Proxy is disabled");
            }
        }
        catch (Exception e)
        {
            MelonLoader.MelonLogger.Error("DebugManager Proxy compilation failed: " + e.Message);
        }
    }

    /// <summary>
    /// Registers new debug command for console.
    /// Usage:
    /// <see cref="DebugManagerProxy.RegisterDebugCommand"/>("mymod_debug_feature1", new Func&lt;string&gt;(Feature1Manager.GetDebugOutput));
    /// type mymod_debug_feature1 command into console to activate debugging, type again to deactivate.
    /// </summary>
    /// <param name="commandName">Command name you want to have in console. Only a-z, 0-9, _ symbols are preferred.</param>
    /// <param name="debugOutputGetter">Function that obtains text to draw. Executes every frame, optimize it!</param>
    /// <exception cref="ArgumentException">Throws on bad arguments.</exception>
    /// <exception cref="DuplicateNameException">Throws if this command is already exists.</exception>
    public delegate void RegisterDebugCommandDelegate(string commandName, Func<string> debugOutputGetter);

    /// <summary>
    /// Unregisters new debug function.
    /// Usage:
    /// DebugManager.UnregisterDebugCommand("mymod_debug_feature1");
    /// </summary>
    /// <param name="commandName">Command name that was registered via <see cref="DebugManagerProxy.RegisterDebugCommand"/>.</param>
    /// <exception cref="ArgumentException">Throws on bad arguments.</exception>
    public delegate void UnregisterDebugCommandDelegate(string commandName);
    
    /// <summary>
    /// Checks the current status of debug command (enabled or not).
    /// </summary>
    /// <param name="commandName">Command name that was registered via <see cref="DebugManagerProxy.RegisterDebugCommand"/>.</param>.
    /// <returns>true if active.</returns>
    public delegate bool GetDebugEnabledDelegate(string commandName);

    /// <summary>
    /// Enables or disables debug command (enabled or not).
    /// </summary>
    /// <param name="commandName">Command name that was registered via <see cref="DebugManagerProxy.RegisterDebugCommand"/>.</param>.
    /// <param name="newStatus">True if active.</param>.
    /// <exception cref="ArgumentException">Throws on bad arguments.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Throws if such a command does not exists.</exception>
    public delegate void SetDebugEnabledDelegate(string commandName, bool newStatus);
}