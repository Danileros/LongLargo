using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Interfaces;
using LongLargo.Managers;
using LongLargo.Model;
using LongLargo.Utils;
using MelonLoader;
using UnityEngine;

namespace LongLargo;

public class Main : MelonMod
{
    // To get soundtracks
    public static IPlaylistManager PlaylistManager { get; private set; }
    
    // To play soundtracks
    public static IAudioPlayer AudioPlayer { get; private set; }
    
    // Timberwolf combat special 
    public static IPackProximityManager PackProximityManager { get; private set; }

    public override void OnInitializeMelon()
    {
        Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");
        SettingsManager.OnLoad();

        PlaylistManager = new PlaylistManager();
        AudioPlayer = new AudioPlayer();
        PackProximityManager = new PackProximityManager();
        AddConsoleCommands();
    }
    
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        // Stops playing danger music
        PackProximityManager.ForceLeaveCombat();
        AudioPlayer.StopIfSituation(SituationType.Stalked);
    }

    public static bool IsModDisabled()
    {
        if (!SettingsManager.Settings.ModEnabled)
        {
            return true;
        }

        var scene = GameManager.m_ActiveScene;
        if (ScenesHelper.IsForbidden(scene) || ScenesHelper.IsTales(scene)) // Don't mess with tales
        {
            return true;
        }

        return false;
    }
    
    private static void AddConsoleCommands()
    {
        uConsole.RegisterCommand("ll_stop", new Action(() => AudioPlayer.Stop()));
        uConsole.RegisterCommand("ll_playlist", new Action(CommandShowPlaylist));
        uConsole.RegisterCommand("ll_play", new Action(CommandPlay));
        uConsole.RegisterCommand("ll_play_next", new Action(CommandPlayNext));
        uConsole.RegisterCommand("ll_silence", new Action(CommandSilence));
        uConsole.RegisterCommand("ll_stinger_weather", new Action(CommandStingerWeather));
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
            uConsole.Log($"No track found with name {trackName}");
            return;
        }
            
        AudioPlayer.PlayHard(soundtrack, SituationType.Disabled);
        uConsole.Log($"Now playing: {soundtrack.TrackName}");
    }
    
    private static void CommandPlayNext()
    {
        var (soundtrack, _)
            = PlaylistManager.GetExplorationSoundtrack(SituationTypeExtensions.GetExplorationSituation(),true);
        AudioPlayer.PlayHard(soundtrack, SituationType.Disabled);
        uConsole.Log($"Now playing: {soundtrack.TrackName}");
    }
    
    private static void CommandSilence()
    {
        var soundtrack = PlaylistManager.ShortSilence;
        AudioPlayer.PlayHard(soundtrack, SituationType.Disabled);
        uConsole.Log($"Now playing: {soundtrack.TrackName}");
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