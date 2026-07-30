using LongLargo.Extensions;
using LongLargo.Helpers;
using LongLargo.Managers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo;

public class Main : MelonMod
{
    public static PlaylistManager PlaylistManager { get; private set; }
        
    public static QueueManager QueueManager { get; private set; }

    public override void OnInitializeMelon()
    {
        Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
        SettingsManager.OnLoad();

        PlaylistManager = new PlaylistManager();
        QueueManager = new QueueManager(PlaylistManager.Soundtracks);
    }
    
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (ScenesHelper.IsForbidden(sceneName))
        {
            return;
        }

        QueueManager.StopIfSituation(SituationTypeExtensions.GetDangers());
    }
}