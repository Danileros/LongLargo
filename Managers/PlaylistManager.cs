using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Interfaces;
using LongLargo.Models;
using LongLargo.Utils;

namespace LongLargo.Managers;

public class PlaylistManager : IPlaylistManager
{
    private SoundtrackInfo[] _soundtracks;
    
    public IReadOnlyCollection<SoundtrackInfo> Soundtracks  => _soundtracks;
    
    // We will play this with default music to track overlaping
    public SoundtrackInfo LongSilence { get; private set; }
    public SoundtrackInfo ShortSilence { get; private set; }

    public PlaylistManager()
    {
        var playlistLoader = new PlaylistLoader();
        playlistLoader.LoadSilence(out var shortSilence, out var longSilence); // TODO: Test if I can get rid of Silence by modifying IsPlaying 
        var loadedSoundtracks  = playlistLoader.LoadAllSources();

        ShortSilence = shortSilence;
        LongSilence = longSilence;
        _soundtracks = loadedSoundtracks.ToArray();
    }

    public SoundtrackInfo GetSoundtrackByName(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            return null;
        }

        name = name.ToLower();
        var filteredTracks = _soundtracks.AsEnumerable();
        if (name.Contains(':'))
        {
            var split = name.Split(':');
            var asset = split[0];
            if (split[0] == "local")
            {
                filteredTracks = filteredTracks.Where(t => t.AssetBundle == null);
            }
            else
            {
                filteredTracks = filteredTracks
                    .Where(t => 
                        t.AssetBundle != null
                        && Path.GetFileNameWithoutExtension(t.AssetBundle.name).ToLower() == asset);
            }
            
            name = split[1];
        }

        var exactMatch = filteredTracks.FirstOrDefault(t => t.TrackName == name);
        if (exactMatch != null)
        {
            return exactMatch;
        }
        else
        {
            var substringMatch = filteredTracks.FirstOrDefault(t => t.TrackName.Contains(name));
            if (substringMatch != null)
            {
                return substringMatch;
            }
            else
            {
                return null;
            }
        }
    }
    
    public (SoundtrackInfo, bool) GetExplorationSoundtrack(FSituationType situation, bool excludeVanillaMusic = false)
    {
        if (IsVanillaOnly(situation))
        {
            return (Main.PlaylistManager.LongSilence, true);
        }
        
        var scene = GameManager.m_ActiveScene;
        var locationType = ScenesHelper.GetLocationType(scene);

        var soundtracks = _soundtracks
            .Where(s =>
                s != Main.AudioPlayer.LastSoundtrack
                && s.SituationsRestrictsTo.HasFlagSafe(situation)
                && s.LocationsTypeRestrictTo.HasFlag(locationType)
                && (s.LocationRestrictTo is null or { Length: 0 } || s.LocationRestrictTo.Contains(scene))
                && (!SettingsManager.Settings.DisableCopyrightedMusic || s.Copyright != true))
            .ToArray();

        var soundtrack = ChooseRandomSoundtrack(soundtracks, situation, excludeVanillaMusic);
        if (soundtrack != null)
        {
            return (soundtrack, false);
        }
        else
        {
            return (Main.PlaylistManager.LongSilence, true);
        }
    }

    public (SoundtrackInfo, bool) GetSituationSoundtrack(FSituationType situation, bool excludeVanillaMusic = false)
    {
        if (IsVanillaOnly(situation))
        {
            return (Main.PlaylistManager.ShortSilence, true);
        }

        var soundtracks = _soundtracks
            .Where(s =>
                s != Main.AudioPlayer.LastSoundtrack
                && s.SituationsRestrictsTo.HasFlagSafe(situation)
                && (!SettingsManager.Settings.DisableCopyrightedMusic || s.Copyright != true))
            .ToArray();

        var soundtrack = ChooseRandomSoundtrack(soundtracks, situation, excludeVanillaMusic);
        if (soundtrack != null)
        {
            return (soundtrack, false);
        }
        else
        {
            return (Main.PlaylistManager.ShortSilence, true);
        }
    }

    private bool IsVanillaOnly(FSituationType situation)
    {
        return
            SettingsManager.Settings.ExplorationMusicMode == EExplorationMusicMode.Vanilla && situation.IsExploration()
            || SettingsManager.Settings.WeatherMode == EWeatherStingerMode.Vanilla && situation.IsWeather()
            || SettingsManager.Settings.StalkedMode == EStalkedMode.Vanilla && (FSituationType.Stalked).HasFlagSafe(situation)
            || SettingsManager.Settings.TimberwolfSuppress && (FSituationType.Timberwolf).HasFlagSafe(situation);
    }

    private SoundtrackInfo ChooseRandomSoundtrack(ICollection<SoundtrackInfo> soundtracks,
        FSituationType situation,
        bool excludeVanillaMusic = false)
    {
        var customWeightsSum = soundtracks.Sum(s => s.Chance);
        var vanillaWeight = CalculateVanillaWeight(situation, excludeVanillaMusic, customWeightsSum);

        var totalWeights = customWeightsSum + vanillaWeight;
        var choosenOne = UnityEngine.Random.Range(0, totalWeights);
        if (choosenOne < vanillaWeight)
        {
            return null; // vanilla
        }
        else
        {
            choosenOne -= vanillaWeight;
            foreach (var soundtrack in soundtracks)
            {
                choosenOne -= soundtrack.Chance;
                if (choosenOne < 0)
                {
                    return soundtrack;
                }
            }
        }
        
        return null;
    }

    private long CalculateVanillaWeight(FSituationType situation, bool excludeVanillaMusic, long customWeightsSum)
    {
        long vanillaWeight;
        if (excludeVanillaMusic)
        {
            vanillaWeight = 0;
        }
        else
        {
            switch (situation)
            {
                case FSituationType.ExplorationDay:
                case FSituationType.ExplorationNight:
                case FSituationType.ExplorationAurora:
                    vanillaWeight = CalculateExplorationWeight(customWeightsSum);
                    break;
                case FSituationType.WeatherClear:
                case FSituationType.WeatherFog:
                case FSituationType.WeatherSnow:
                case FSituationType.WeatherBlizzard:
                    vanillaWeight = CalculateWeatherWeight(customWeightsSum);
                    break;
                case FSituationType.Stalked:
                    vanillaWeight = CalculateStalkedWeight(customWeightsSum);
                    break;
                case FSituationType.Timberwolf:
                    vanillaWeight = 0;
                    break;
                default:
                    vanillaWeight = 100;
                    break;
            }
        }

        return vanillaWeight;
    }

    private long CalculateExplorationWeight(long customWeightsSum)
    {
        switch(SettingsManager.Settings.ExplorationMusicMode)
        {
            case EExplorationMusicMode.Suppress:
            case EExplorationMusicMode.CustomOnly:
                return 0;
            case EExplorationMusicMode.X05:
                return 50;
            case EExplorationMusicMode.Default:
                return 100;
            case EExplorationMusicMode.X2:
                return 200;
            case EExplorationMusicMode.X5:
                return 500;
            case EExplorationMusicMode.X10:
                return 1000;
            case EExplorationMusicMode.Balanced:
                return customWeightsSum;
            default:
                return 0;
        }
    }

    private long CalculateWeatherWeight(long customWeightsSum)
    {
        switch(SettingsManager.Settings.WeatherMode)
        {
            case EWeatherStingerMode.Suppress:
            case EWeatherStingerMode.CustomOnly:
                return 0;
            case EWeatherStingerMode.Default:
                return 100;
            case EWeatherStingerMode.Balanced:
                return customWeightsSum;
            default:
                return 0;
        }
    }

    private long CalculateStalkedWeight(long customWeightsSum)
    {
        switch(SettingsManager.Settings.StalkedMode)
        {
            case EStalkedMode.Suppress:
            case EStalkedMode.Wintermute:
                return 0;
            case EStalkedMode.Default:
                return 100;
            default:
                return 0;
        }
    }
}