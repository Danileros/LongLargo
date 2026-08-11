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
    
    public (SoundtrackInfo, bool) GetExplorationSoundtrack(SituationType situation, bool notVanilla = false)
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
                && (s.LocationRestrictTo is null or { Length: 0 } || s.LocationRestrictTo.Contains(scene)))
            .ToArray();

        var soundtrack = ChooseRandomSoundtrack(soundtracks, notVanilla);
        if (soundtrack != null)
        {
            return (soundtrack, false);
        }
        else
        {
            return (Main.PlaylistManager.LongSilence, true);
        }
    }

    public (SoundtrackInfo, bool) GetSituationSoundtrack(SituationType situation, bool notVanilla = false)
    {
        if (IsVanillaOnly(situation))
        {
            return (Main.PlaylistManager.ShortSilence, true);
        }

        var soundtracks = _soundtracks
            .Where(s =>
                s != Main.AudioPlayer.LastSoundtrack
                && s.SituationsRestrictsTo.HasFlagSafe(situation))
            .ToArray();

        var soundtrack = ChooseRandomSoundtrack(soundtracks, notVanilla);
        if (soundtrack != null)
        {
            return (soundtrack, false);
        }
        else
        {
            return (Main.PlaylistManager.ShortSilence, true);
        }
    }

    private bool IsVanillaOnly(SituationType situation)
    {
        return
            SettingsManager.Settings.ExplorationVanillaOnly && situation.IsExploration()
            || SettingsManager.Settings.WeatherVanillaOnly && situation.IsWeather()
            || SettingsManager.Settings.TimeVanillaOnly && situation.IsTime()
            || SettingsManager.Settings.StalkedSuppress && (SituationType.Stalked).HasFlagSafe(situation)
            || SettingsManager.Settings.TimberwolfSuppress && (SituationType.Timberwolf).HasFlagSafe(situation)
            || SettingsManager.Settings.ConditionVanillaOnly && situation.IsCondition();
    }

    private SoundtrackInfo ChooseRandomSoundtrack(ICollection<SoundtrackInfo> soundtracks, bool notVanilla = false)
    {
        var vanillaChance = notVanilla ? 0 : SettingsManager.Settings.ModVanillaMusicChance;
        var sum = soundtracks.Sum(s => s.Chance) + vanillaChance;
        var choosenOne = UnityEngine.Random.Range(0, sum);
        if (choosenOne < vanillaChance)
        {
            return null; // vanilla
        }
        else
        {
            choosenOne -= vanillaChance;
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
}