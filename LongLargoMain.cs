using System.IO;
using LongLargo.Handlers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo;

public class LongLargoMain : MelonMod
{
    public static PlaylistProvider PlaylistProvider { get; private set; }
        
    public static QueueManager QueueManager { get; private set; }

    public static bool DebugMode { get; private set; }

    public override void OnInitializeMelon()
    {
        Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
        if (File.Exists(Path.Combine(PlaylistProvider.FolderPath, "LLDebug")))
        {
            DebugMode = true;
        }

        LLSettings.OnLoad();

        PlaylistProvider = new PlaylistProvider();
        QueueManager = new QueueManager(PlaylistProvider.Soundtracks);
    }
    
    // public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    // {
    //     if (sceneName == "Empty")
    //     {
    //         return;
    //     }
    //
    //     if (sceneName.Contains("Boot") || sceneName.Contains("Menu"))
    //     {
    //         QueueManager.Pause();
    //     }
    //     else
    //     {
    //         QueueManager.Resume();
    //     }
    // }
}