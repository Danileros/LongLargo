using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AudioMgr;
using LongLargo.Model;
using NAudio.Wave;
using UnityEngine;

namespace LongLargo.Handlers;

/// <summary>
/// Loads and provides playlist and tracks.
/// </summary>
public class PlaylistManager
{
    public SoundtrackInfo[] Soundtracks { get; private set; }
    
    // We will play this with default music to track overlaping
    public Clip LongSilence { get; private set; }
    public Clip ShortSilence { get; private set; }
    
    internal static string FolderName { get; } = "LongLargo";
    internal static string FolderPath { get; } = Application.dataPath + "/../Mods/" + FolderName;

    private readonly JsonSerializerOptions _playlistSerializerOptions =
        new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            IgnoreNullValues = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            AllowTrailingCommas = true,
            Converters = { new SafeJsonEnumConverterFactory() }
        };
    
    // Different clipmanagers to avoid filename collision.
    private readonly List<AssetBundle> _loadedAssets = new List<AssetBundle>();
    private readonly List<ClipManager> _loadedClipManagers = new List<ClipManager>();

    public PlaylistManager()
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        LoadSilence(); // TODO: Test if I can get rid of Silence by modifying IsPlaying 
        
        var loadedSoundtracks  = new List<SoundtrackInfo>();
        LoadLlAudio(loadedSoundtracks);
        LoadSoundracksFromDisk(loadedSoundtracks);
        LoadAssetBundlesFromDisk(loadedSoundtracks);
        
        Soundtracks = loadedSoundtracks.ToArray();
    }

    /// <summary>
    /// Get audioclip for soundtrack.
    /// </summary>
    /// <param name="soundtrack">Soundtrack info.</param>
    /// <returns>Clip.</returns>
    public Clip GetClip(SoundtrackInfo soundtrack)
    {
        var assetIndex = this._loadedAssets.FindIndex(x => soundtrack.AssetBundle == x);
        var clipManager = _loadedClipManagers[assetIndex];
        return clipManager.GetClip(soundtrack.TrackName);
    }

    // Technical tracks with no sound.
    private void LoadSilence()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LongLargo.silence.unity3d");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var array = ms.ToArray();
        var ilStream = new Il2CppSystem.IO.MemoryStream(array);
        
        // I wish LoadFromMemory to work but no 
        var assetBundle = AssetBundle.LoadFromStream(ilStream);
        var names = assetBundle.GetAllAssetNames();
        var silenceManager = AudioMaster.NewClipManager();
        silenceManager.LoadAudioclip("ShortSilence", assetBundle.LoadAsset<AudioClip>("shortsilence.ogg"));
        silenceManager.LoadAudioclip("LongSilence", assetBundle.LoadAsset<AudioClip>("longsilence.ogg"));
        ShortSilence = silenceManager.GetClip("ShortSilence");  // for stingers
        LongSilence = silenceManager.GetClip("LongSilence");    // for music
    }
    
    // Load local soundtracks provided by user.
    private void LoadSoundracksFromDisk(List<SoundtrackInfo> loadedSoundtracks)
    {
        var soundtrackPaths = Directory.GetFiles(FolderPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(p =>
            {
                var ext = Path.GetExtension(p);
                return ext == ".wav" || ext == ".ogg" || ext == ".mp3";
            })
            .ToArray();
        var localPlaylist = LoadLocalPlaylist(soundtrackPaths);
        var clipManager = AudioMaster.NewClipManager();
        try
        {
            if (LLSettings.settings.EnableUglyLoad)
            {
                LoadUgly(soundtrackPaths, clipManager);
            }
            else
            {
                foreach (var path in soundtrackPaths)
                {
                    clipManager.LoadClipFromFile(
                        GetTrackName(path),
                        path,
                        ClipManager.LoadType.Stream);
                }
            }
        }
        catch (Exception e)
        {
            LLogger.Error($"Failed loading soundtracks using AudioManager, error: {e.Message}");

            return;
        }
        
        ExtractClips(loadedSoundtracks, null, clipManager, localPlaylist); // null = local files
    }

    // Load user PlaylistInfo.json and probably update
    private PlaylistInfo LoadLocalPlaylist(string[] soundtrackPaths)
    {
        var jsonPath = Path.Combine(FolderPath, "PlaylistInfo.json");
        PlaylistInfo localPlaylist;
        if (File.Exists(jsonPath))
        {
            try
            {
                using FileStream openStream = File.OpenRead(jsonPath);
                localPlaylist = JsonSerializer.Deserialize<PlaylistInfo>(openStream, _playlistSerializerOptions);
            }
            catch (Exception e)
            {
                LLogger.Error("Detected broken PlaylistInfo.json, recreating. Sad :(");
                localPlaylist = new PlaylistInfo();
            }
        }
        else
        {
            localPlaylist = new PlaylistInfo();
        }
        
        var soundtrackNames
            = soundtrackPaths.Select(path => GetTrackName(path));
        var deleted = localPlaylist.SoundtrackInfos
            .RemoveAll(p => string.IsNullOrEmpty(p.TrackName) || !soundtrackNames.Contains(p.TrackName));
        
        var missingSoundtracks = soundtrackNames
            .Where(p => !localPlaylist.SoundtrackInfos
                .Any(s => s.TrackName.Equals(p, StringComparison.InvariantCultureIgnoreCase)))
            .Select(p => new SoundtrackInfo()
            {
                TrackName = p,
            })
            .ToArray();
        localPlaylist.SoundtrackInfos.AddRange(missingSoundtracks);

        if (missingSoundtracks.Length > 0 || deleted > 0)
        {
            LLogger.Log("Playlist mismatch detected, updating PlaylistInfo.json");
            WriteUpdatedPlaylist(localPlaylist, jsonPath);
        }

        return localPlaylist;
    }

    private void WriteUpdatedPlaylist(PlaylistInfo localPlaylist, string jsonPath)
    {
        var header = @"
This is the Long Largo Playlist Info. It contains all the tracks that are exists in this folder.
If you will drop any ogg/wav/mp3 audio file, it will be added to this file automatically on next game start.
Note that this file is recreated automatically each game start, so keep in mind changes may be lost if it cannot be parsed.
Fields:
  Chance - Chance to play (default is 100), higher is more often, 0 is disabled.
  SituationsRestrictsTo - Play only when specific conditions are meet (like good weather).
    Example:
      ""SituationsRestrictsTo"": ""ExplorationDay, ExplorationNight"",
    Possible values:
    Disabled
    -- Exploration soundtrack are supposed to be long and play when you are exploring the world
    ExplorationDay
    ExplorationNight
    ExplorationAurora
    -- Extra short soundtracks (<30s) that are bound to specific weather
    WeatherClear
    WeatherFog
    WeatherSnow
    WeatherBlizzard
    -- Extra short soundtracks (<30s) that are bound to specific time
    TimeDusk
    TimeDawn
    -- Short soundtracks (<1m) when animal hunts you, plays looped
    Stalked
    Timberwolf
    -- Extra short soundtracks (<30s) that are bound to specific condition
    ConditionSuccess
    ConditionSorrow
  LocationsTypeRestrictTo - Play only at specific location type (only for Exploration* soundtracks). If Any, plays at any location type.
    Example:
      ""LocationsTypeRestrictTo"": ""Region, TransitionZone"",
    Possible values:
    Any
    Disabled
    Region
    TransitionZone
    Building
    Cave
    Mine
  LocationRestrictTo - Play only at specific locations (only for Exploration* soundtracks). If empty, plays at any location. You can get a location name from Latest.log.
    Example: 
      ""LocationRestrictTo"": [
        ""BlackrockRegion"",
        ""BlackrockPrisonSurvivalZone""
      ]
    Possible values:
    AshCanyonRegion       	    Ash Canyon
    BlackrockRegion             Blackrock
    CanneryRegion 	            Bleak Inlet
    TracksRegion 	            Broken Railroad
    CoastalRegion 	            Coastal Highway
    WhalingStationRegion 	    Desolation Point
    MarshRegion          	    Forlorn Muskeg
    RiverValleyRegion    	    Hushed River Valley
    MountainTownRegion 	        Mountain Town
    LakeRegion 	                Mystery Lake
    HighwayTransitionZone 	    Old Island Connector
    RuralRegion 	            Pleasant Valley
    RavineTransitionZone 	    Raven Falls Railway Line (Ravine)
    CrashMountainRegion 	    Timberwolf Mountain
    DamRiverTransitionZoneB 	Winding River
    LongRailTransitionZone      Far Range Branch Line
    HubRegion                   Transfer Pass
    AirfieldRegion              Forsaken Airfield
    MiningRegion                Zone of Contamination
    MountainPassRegion          Sundered Pass
";
        
        using (var fs = new FileStream(jsonPath, FileMode.Create, FileAccess.Write))
        {
            using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions{ Indented = true }))
            {
                writer.WriteCommentValue(header);
                JsonSerializer.Serialize(writer, localPlaylist, _playlistSerializerOptions);
                writer.Flush();
            }
        }
    }

    private void LoadLlAudio(List<SoundtrackInfo> loadedSoundtracks)
    {
        var naudio = Assembly.LoadFrom(Path.Combine(FolderPath, "NAudio.dll"));
        LLogger.Log("Loaded NAudio");
        
        var rawPaths = Directory.GetFiles(FolderPath, "*.raw", SearchOption.TopDirectoryOnly);
        var ilStreams = rawPaths
            .Select(File.ReadAllBytes)
            .Where(x => x.Length != 0)
            .Select(array =>
            {
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = (byte)(array[i] ^ 0xAA);
                }
                
                return array;
            })
            .Select(array =>
                new Il2CppSystem.IO.MemoryStream(array)
            )
            .ToArray();
        foreach (var ilStream in ilStreams)
        {
            LoadAssetBundle(loadedSoundtracks, AssetBundle.LoadFromStream(ilStream));
        }
    }

    private void LoadAssetBundlesFromDisk(List<SoundtrackInfo> loadedSoundtracks)
    {
        var assetsPaths = Directory.GetFiles(FolderPath, "*.unity3d", SearchOption.TopDirectoryOnly);
        var assets = assetsPaths.Select(AssetBundle.LoadFromFile).Where(x => x != null).ToArray();
        foreach (var assetBundle in assets)
        {
            LoadAssetBundle(loadedSoundtracks, assetBundle);
        }
    }

    private void LoadAssetBundle(List<SoundtrackInfo> loadedSoundtracks, AssetBundle assetBundle)
    {
        LLogger.Log("Asset Bundle loaded: {0}", assetBundle.name);
        var jsonAsset = assetBundle.LoadAsset<TextAsset>("PlaylistInfo.json");
        if (!jsonAsset)
        {
            LLogger.Error("Asset Bundle does not have PlaylistInfo.json, skipping");
            return;
        }
            
        PlaylistInfo playlist;
        try
        {
            playlist = JsonSerializer.Deserialize<PlaylistInfo>(jsonAsset.text, _playlistSerializerOptions);
        }
        catch (Exception e)
        {
            LLogger.Error("Asset Bundle's PlaylistInfo.json can't be deserialized, skipping");
            return;
        }
            
        var clipManager = AudioMaster.NewClipManager();
        clipManager.LoadAllClipsFromBundle(assetBundle);
        ExtractClips(loadedSoundtracks, assetBundle, clipManager, playlist);
        LogAssetNames(assetBundle);
    }

    private void LoadUgly(string[] soundtrackPaths, ClipManager clipManager)
    {
        if (soundtrackPaths.Length == 0)
        {
            return;
        }

        foreach (var path in soundtrackPaths)
        {
            var (pcmData, channels, sampleRate) = LoadAudioAsFloat(path);
            var name = GetTrackName(path);
            var audioClip = LoadFromRawPCM(name, pcmData, channels, sampleRate);
            clipManager.LoadAudioclip(name, audioClip);
        }
    }

    // TODO: remove when Melon or AudioManager will get an update
    private (float[],int, int) LoadAudioAsFloat(string filePath)
    {
        using (var reader = new AudioFileReader(filePath))
        {
            // Total number of samples = length in bytes / bytes per sample (4 for float)
            var sampleCount = (int)(reader.Length / 4);
            var buffer = new float[sampleCount];
    
            // Read all samples into the array
            var read = reader.Read(buffer, 0, buffer.Length);
            var channels = reader.WaveFormat.Channels;
            var sampleRate = reader.WaveFormat.SampleRate;
            return (buffer, channels, sampleRate);
        }
    }
    
    private AudioClip LoadFromRawPCM(string name, float[] pcmData, int channels, int sampleRate)
    {
        var clip = AudioClip.Create(name, pcmData.Length / channels, channels, sampleRate, false);
        clip.SetData(pcmData, 0);
    
        return clip;
    }
    
    private void ExtractClips(List<SoundtrackInfo> loadedSoundtracks, AssetBundle assetBundle, ClipManager clipManager, PlaylistInfo playlist)
    {
        _loadedAssets.Add(assetBundle);
        _loadedClipManagers.Add(clipManager);
        var i = 0;
        foreach (var soundtrackInfo in playlist.SoundtrackInfos)
        {
            soundtrackInfo.AssetBundle = assetBundle;
            if (clipManager.GetClip(soundtrackInfo.TrackName) != null)
            {
                ++i;
                loadedSoundtracks.Add(soundtrackInfo);
            }
            else
            {
                LLogger.Error($"Failed loading soundtrack {soundtrackInfo.TrackName} using AudioManager");
            }
        }
        
        LLogger.Log($"Loaded {i} soundtracks from {(!assetBundle ? "local folder" : assetBundle.name)}");
    }

    private static string GetTrackName(string path)
    {
        return Path.GetFileNameWithoutExtension(path).ToLower(CultureInfo.CurrentCulture);
    }

    private static void LogAssetNames(AssetBundle assetBundle)
    {
        foreach (string assetName in assetBundle.GetAllAssetNames())
        {
            LLogger.Debug($"Assets: {assetName}");
        }
    }
}