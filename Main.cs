using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Interfaces;
using LongLargo.Managers;
using LongLargo.Models;
using LongLargo.Utils;
using MelonLoader;
using UnityEngine;

namespace LongLargo;

public class Main : MelonMod
{
    private string _previousSceneName;
    
    // To get soundtracks
    public static IPlaylistManager PlaylistManager { get; private set; }
    
    // To play soundtracks
    public static IAudioPlayer AudioPlayer { get; private set; }
    
    // Timberwolf combat special 
    public static IPackCombatManager PackCombatManager { get; private set; }
    
    public static DebugManagerProxy DebugManager { get; private set; }

    public override void OnInitializeMelon()
    {
        Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
        SettingsManager.OnLoad();

        DebugManager = new DebugManagerProxy();
        PlaylistManager = new PlaylistManager();
        AudioPlayer = new AudioPlayer();
        PackCombatManager = new PackCombatManager(EPackProximityRange.Default);
        AddConsoleCommands();
    }

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        if (ScenesHelper.IsSubscene(sceneName))
        {
            base.OnSceneWasUnloaded(buildIndex, sceneName);
            return;
        }
        
        LLogger.Debug($"[Main] Scene {sceneName} unloaded");

        // Stops playing danger music
        PackCombatManager.ForceLeaveCombat();
        AudioPlayer.StopIfSituation(FSituationType.Stalked, 1f);
        AudioPlayer.StopIfSilence();
        _previousSceneName = sceneName;
        
        base.OnSceneWasUnloaded(buildIndex, sceneName);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (ScenesHelper.IsMenu(sceneName) || ScenesHelper.IsTales(sceneName))
        {
            Main.AudioPlayer.Stop(1f);
            base.OnSceneWasLoaded(buildIndex, sceneName);
            return;
        }
        
        if (ScenesHelper.IsSubscene(sceneName))
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            return;
        }
        
        LLogger.Debug($"[Main] Scene {sceneName} loaded");

        if (StopExplorationMusicOnTransit(sceneName))
        {
            Main.AudioPlayer.StopIfSituation(SituationTypeExtensions.GetExplorations(), 1f);
        }

        base.OnSceneWasLoaded(buildIndex, sceneName);
    }

    // Check if we are entering/exiting buildings.
    // Most buildings has no exploration music so LL should not stop playing each time player visiting a small cabin.
    private bool StopExplorationMusicOnTransit(string sceneName)
    {
        var locationFrom = ScenesHelper.GetLocationType(_previousSceneName);
        var locationTo = ScenesHelper.GetLocationType(sceneName);

        if (locationFrom == FLocationType.Building || locationTo == FLocationType.Building)
        {
            var buildingScene = locationFrom == FLocationType.Building
                ? _previousSceneName
                : sceneName;

            // However, Dam and Steam Tunnels somehow HAS exploration music so LL should stop music this time
            if (!ScenesHelper.IsExplorationBuilding(buildingScene))
            {
                return false;
            }
        }

        return true;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        var keyPlayNext = SettingsManager.Settings.KeyPlayNext;
        if (keyPlayNext != KeyCode.None && Input.GetKeyDown(keyPlayNext))
        {
            CommandPlayNext();
        }

        var keyPlayStop = SettingsManager.Settings.KeyStop;
        if (keyPlayStop != KeyCode.None && Input.GetKeyDown(keyPlayStop))
        {
            AudioPlayer.Stop();
        }

        var keyPlayLast = SettingsManager.Settings.KeyPlayLast;
        if (keyPlayLast != KeyCode.None && Input.GetKeyDown(keyPlayLast))
        {
            CommandPlayLast();
        }
    }

    public static bool IsModDisabled()
    {
        if (!SettingsManager.Settings.ModEnabled)
        {
            return true;
        }

        var scene = GameManager.m_ActiveScene;
        if (ScenesHelper.IsMenu(scene) || ScenesHelper.IsTales(scene)) // Don't mess with tales
        {
            return true;
        }

        return false;
    }
    
    private static void AddConsoleCommands()
    {
        uConsole.RegisterCommand("ll_stop", new Action(CommandStop));
        uConsole.RegisterCommand("ll_playlist", new Action(CommandShowPlaylist));
        uConsole.RegisterCommand("ll_play", new Action(CommandPlay));
        uConsole.RegisterCommand("ll_play_next", new Action(CommandPlayNext));
        uConsole.RegisterCommand("ll_play_last", new Action(CommandPlayLast));
        uConsole.RegisterCommand("ll_silence", new Action(CommandSilence));
        uConsole.RegisterCommand("ll_stinger_weather", new Action(CommandStingerWeather));
    }

    private static void CommandStop()
    {
        AudioPlayer.Stop();
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
        var soundtrack = PlaylistManager.GetSoundtrackByName(trackName);
        if (soundtrack == null)
        {
            uConsole.Log($"[LL] No track found with name {trackName}");
            return;
        }

        if (soundtrack.Copyright == true && SettingsManager.Settings.DisableCopyrightedMusic)
        {
            uConsole.Log($"[LL] 'Disable Copyrighted Music' restricts playing {trackName}.");
            return;
        }
            
        AudioPlayer.PlayHard(soundtrack, FSituationType.Disabled);
    }
    
    private static void CommandPlayNext()
    {
        var (soundtrack, _)
            = PlaylistManager.GetExplorationSoundtrack(SituationTypeExtensions.GetExplorationSituation(),true);
        AudioPlayer.PlayHard(soundtrack, FSituationType.Disabled);
    }
    
    private static void CommandPlayLast()
    {
        AudioPlayer.PlayHard(AudioPlayer.LastSoundtrack, FSituationType.Disabled);
    }
    
    private static void CommandSilence()
    {
        var soundtrack = PlaylistManager.ShortSilence;
        AudioPlayer.PlayHard(soundtrack, FSituationType.Disabled);
    }

    private static void CommandStingerWeather()
    {
        var stage = GameManager.GetWeatherComponent().GetWeatherStage();
        switch (stage)
        {
            case WeatherStage.LightSnow:
            case WeatherStage.HeavySnow:
                GameAudioManager.PlaySound("Play_Weather_Clear_withStinger45", GameManager.GetVpFPSPlayer().gameObject);
                return;
            case WeatherStage.PartlyCloudy:
            case WeatherStage.Cloudy:
            case WeatherStage.Clear:
                GameAudioManager.PlaySound("Play_Weather_Clear_withStinger60", GameManager.GetVpFPSPlayer().gameObject);
                return;
            case WeatherStage.ClearAurora:
                GameAudioManager.PlaySound("Play_Weather_ClearAurora_withStinger60", GameManager.GetVpFPSPlayer().gameObject);
                return;
            case WeatherStage.Blizzard:
                GameAudioManager.PlaySound("Play_Weather_Blizzard_withStinger60", GameManager.GetVpFPSPlayer().gameObject);
                return;
            case WeatherStage.LightFog:
                GameAudioManager.PlaySound("Play_Weather_LightFog_withStinger45", GameManager.GetVpFPSPlayer().gameObject);
                return;
            case WeatherStage.DenseFog:
                GameAudioManager.PlaySound("Play_Weather_DenseFog_withStinger45", GameManager.GetVpFPSPlayer().gameObject);
                return;
            case WeatherStage.ToxicFog:
            case WeatherStage.ElectrostaticFog:
                GameAudioManager.PlaySound("Play_Weather_ElectrostaticFog", GameManager.GetVpFPSPlayer().gameObject);
                return;
        }
    }
}