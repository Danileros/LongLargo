using Il2Cpp;
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
        AddConsoleCommands();
    }
    
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (ScenesHelper.IsForbidden(sceneName))
        {
            return;
        }

        QueueManager.StopIfSituation(SituationTypeExtensions.GetDangers());
    }
    
    private static void AddConsoleCommands()
    {
        uConsole.RegisterCommand("ll_stop", new Action(() => QueueManager.Stop()));
        uConsole.RegisterCommand("ll_playlist", new Action(CommandShowPlaylist));
        uConsole.RegisterCommand("ll_play", new Action(CommandPlay));
        uConsole.RegisterCommand("ll_play_next", new Action(CommandPlayNext));
    }


    private static void CommandShowPlaylist()
    {
        foreach (var source in PlaylistManager.Soundtracks.GroupBy(t => t.AssetBundle).ToArray())
        {
            string bundleName;
            if (source.Key == null)
            {
                bundleName = "local";
            }
            else
            {
                bundleName = Path.GetFileNameWithoutExtension(source.Key.name);
            }
            
            uConsole.Log($"{source.Count()} soundtracks in source {bundleName}:");
            var i = 0;
            foreach (var track in source)
            {
                uConsole.Log($"\t{++i}. {track.TrackName}");
            }
        }
    }
    
    private static void CommandPlay()
    {
        if (uConsole.GetNumParameters() != 1)
        {
            uConsoleLog.Add("'ll_play' should contain a track name or part of it. You can get it with ll_playlist.");
            return;
        }

        var trackName = uConsole.GetString();
        var clip = QueueManager.GetClipByName(trackName);
        if (clip == null)
        {
            uConsole.Log($"No track found with name {trackName}");
            return;
        }
            
        QueueManager.PlayHard(clip);
        uConsole.Log($"Playing soundtrack {clip.audioClip.name}");
    }
    
    private static void CommandPlayNext()
    {
        var (clip, allowVanilla) = QueueManager.GetExplorationClip(true);
        QueueManager.PlayHard(clip);
        uConsole.Log($"Playing soundtrack {clip.audioClip.name}");
    }
}