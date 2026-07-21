using LongLargo.Extensions;
using LongLargo.Handlers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo;

public class LongLargoMain : MelonMod
{
    public static PlaylistProvider PlaylistProvider { get; private set; }
        
    public static QueueManager QueueManager { get; private set; }

    public override void OnInitializeMelon()
    {
        Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
        LLSettings.OnLoad();

        PlaylistProvider = new PlaylistProvider();
        QueueManager = new QueueManager(PlaylistProvider.Soundtracks);
    }
    
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == null || sceneName == "Empty" || sceneName.Contains("Menu") || sceneName.Contains("Boot"))
        {
            return;
        }
        
        QueueManager.StopIfSituation(SituationTypeExtensions.GetDangers());
    }
}