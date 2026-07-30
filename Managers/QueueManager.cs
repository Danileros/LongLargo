using System.Collections;
using AudioMgr;
using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Helpers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo.Managers;

/// <summary>
/// Handles soundtracks queue. Chooses which track to play by itself.
/// </summary>
public class QueueManager
{
    private readonly SoundtrackInfo[] _soundtracks;
    private static Shot _shot;
    private static VolumeMaster.OnVolumeChange _onVolumeChange;
    private bool isPaused = false;
    private object _lastPlayToken;
    private object _lastFadeToken;
    
    public bool IsFading { get; private set; }

    public SituationType LastSituation { get; private set; }

    public SoundtrackInfo LastSoundtrack { get; private set; }

    public bool IsPaused => isPaused;

    public bool IsPlaying => Shot._audioSource.isPlaying;

    public static Shot Shot
    {
        get
        {
            if (!_shot)
            {
                lock (typeof(QueueManager))
                {
                    if (!_shot)
                    {
                        try
                        {
                            AudioMaster.CreateMasterParent();
                            _shot = AudioMaster.CreatePlayerShot(AudioMaster.SourceType.BGM);
                            _onVolumeChange = ResetVolume;
                            VolumeMaster.onVolumeChange += _onVolumeChange; // Now I'm in control!
                            ResetVolume();
                        }
                        catch (Exception)
                        {
                            // Sometimes appears when exiting the game by Alt+F4, no reason to worry
                        }
                    }
                }
            }
            
            return _shot;
        }
    }

    public static void ResetVolume()
    {
        if (SettingsManager.Settings.BgmVolumeEnabled)
        {
            var masterVolume = InterfaceManager.GetPanel<Panel_OptionsMenu>().State.m_MasterVolume;
            Shot.SetVolume(masterVolume * SettingsManager.Settings.BgmVolume / 100f);
        }
        else
        {
            Shot.SetVolume(VolumeMaster.GetVolume(AudioMaster.SourceType.BGM));
        }
    }

    public QueueManager(SoundtrackInfo[] soundtracks)
    {
        _soundtracks = soundtracks ?? throw new ArgumentNullException(nameof(soundtracks));
    }

    // Forget last soundtrack (to enable playing same track again)
    public void ResetLastSoundtrack()
    {
        LastSoundtrack = null;
    }

    public void SetVolume(float volume)
    {
        Shot.SetVolume(volume);
    }
    
    /// <summary>
    /// Tries to play and gently refuses if something is playing already.
    /// </summary>
    /// <param name="clip">Clip.</param>
    /// <param name="loop">true if it should be looped.</param>
    public void PlaySoft(Clip clip, bool loop = false)
    {
        isPaused = false;
        if (clip != null && !IsPlaying)
        {
            Stop();
            LLogger.Log($"[Queue] Now playing: {clip.audioClip.name}");
            PlayInternal(clip, loop);
        }
    }

    /// <summary>
    /// Tries to play with delay and gently refuses if something is playing already.
    /// </summary>
    /// <param name="clip">Clip.</param>
    /// <param name="loop">true if it should be looped.</param>
    public void PlaySoftDelayed(Clip clip, float delay)
    {
        isPaused = false;
        if (clip != null && !IsPlaying)
        {
            Stop();
            LLogger.Log($"[Queue] Scheduled after {delay}: {clip.audioClip.name}");
            Shot._audioSource.loop = false;
            _lastPlayToken = MelonCoroutines.Start(this.PlayDelayedRoutine(clip, delay));
        }
    }

    /// <summary>
    /// Stops whatever we play already and plays.
    /// </summary>
    /// <param name="clip">Clip.</param>
    /// <param name="loop">true if it should be looped.</param>
    public void PlayHard(Clip clip, bool loop = false)
    {
        isPaused = false;
        if (clip != null)
        {
            Stop();
            LLogger.Log($"[Queue] Now playing hard: {clip.audioClip.name}");
            PlayInternal(clip, loop);
        }
    }

    /// <summary>
    /// Stops whatever we play already and plays.
    /// </summary>
    /// <param name="clip">Clip.</param>
    /// <param name="fadeOut">time to fade out current track (if any).</param>
    public void PlayHard(Clip clip, float fadeOut)
    {
        isPaused = false;
        if (clip != null)
        {
            LLogger.Log($"[Queue] Now playing hard with fade previous: {clip.audioClip.name}");
            if (!IsPlaying)
            {
                PlayInternal(clip, false);
            }
            else
            {
                _lastPlayToken = MelonCoroutines.Start(this.PlayAfterFade(clip, fadeOut));
            }
        }
    }

    private void PlayInternal(Clip clip, bool loop)
    {
        Shot.AssignClip(clip);
        Shot._audioSource.loop = loop;
        if (clip == Main.PlaylistManager.LongSilence || clip == Main.PlaylistManager.ShortSilence)
        {
            Shot.Play(); // no need to prefetch
            return;
        }
            
        if (loop)
        {
            _lastPlayToken = MelonCoroutines.Start(this.PlayDelayedRoutine(clip, 0.6f));
        }
        else
        {
            Shot.Play(clip);
        }
    }

    public void Stop()
    {
        if (SettingsManager.Settings.DebugMode)
        {
            LLogger.Debug("[Queue] Stopping");
            LLogger.Debug(new System.Diagnostics.StackTrace(true).ToString());
        }
        
        isPaused = false;
        Shot._audioSource.loop = false;
        Shot.Stop();
        IsFading = false;
        if (_lastPlayToken != null)
        {
            MelonCoroutines.Stop(_lastPlayToken);
        }
        
        if (_lastFadeToken != null)
        {
            MelonCoroutines.Stop(_lastFadeToken);
        }
    }

    public void Stop(float fadeOut)
    {
        if (!IsPlaying)
        {
            LLogger.Debug($"[Queue] Stop rejected, nothing to stop");
            return;
        }
        
        if (!IsFading)
        {
            if (SettingsManager.Settings.DebugMode)
            {
                LLogger.Debug($"[Queue] Stopping with fade out {fadeOut:N}");
                LLogger.Debug(new System.Diagnostics.StackTrace(true).ToString());
            }
            
            _lastFadeToken = MelonCoroutines.Start(this.StopRoutine(fadeOut));
        }
        else
        {
            LLogger.Debug($"[Queue] Stop rejected, already in process");
        }
    }

    public void StopIfSituation(SituationType situations, float fadeOut = 0)
    {
        if (!IsPlaying)
        {
            return;
        }
        
        if (situations.HasFlag(LastSituation))
        {
            if (fadeOut > 0)
            {
                Stop(fadeOut);
            }
            else
            {
                Stop();
            }

            return;
        }
        
        LLogger.Debug($"[Queue] Stop rejected for {situations}, current is {LastSituation}");
    }

    public void Pause()
    {
        if (Main.QueueManager.IsPlaying)
        {
            isPaused = true;
            Shot._audioSource.Pause();
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            isPaused = false;
            Shot._audioSource.UnPause();
        }
    }
    
    /// <summary>
    /// Gets random exploration clip.
    /// </summary>
    /// <returns>(Clip, playVanilla)</returns>
    public (Clip, bool) GetExplorationClip()
    {
        var situation = SituationTypeExtensions.GetExplorationSituation();
        LastSituation = situation;
        if (Disabled(situation))
        {
            return (Main.PlaylistManager.LongSilence, true);
        }
        
        var scene = GameManager.m_ActiveScene;
        var locationType = GetLocationType(scene);

        var soundtracks = _soundtracks
            .Where(s =>
                s != LastSoundtrack
                && s.SituationsRestrictsTo.HasFlag(situation)
                && s.LocationsTypeRestrictTo.HasFlag(locationType)
                && (s.LocationRestrictTo is null or { Length: 0 } || s.LocationRestrictTo.Contains(scene)))
            .ToArray();

        var soundtrack = ChooseRandomSoundtrack(soundtracks);
        if (soundtrack != null)
        {
            return (soundtrack, false);
        }
        else
        {
            return (Main.PlaylistManager.LongSilence, true);
        }
    }

    /// <summary>
    /// Gets random situation clip.
    /// </summary>
    /// <returns>(Clip, playVanilla)</returns>
    public (Clip, bool) GetSituationClip(SituationType situation)
    {
        LastSituation = situation;
        if (Disabled(situation))
        {
            return (Main.PlaylistManager.ShortSilence, true);
        }

        var soundtracks = _soundtracks
            .Where(s =>
                s != LastSoundtrack
                && s.SituationsRestrictsTo.HasFlag(situation))
            .ToArray();

        var soundtrack = ChooseRandomSoundtrack(soundtracks);
        if (soundtrack != null)
        {
            return (soundtrack, false);
        }
        else
        {
            return (Main.PlaylistManager.ShortSilence, true);
        }
    }

    private IEnumerator PlayDelayedRoutine(Clip audioClip, float delay)
    {
        Shot.AssignClip(Main.PlaylistManager.ShortSilence);
        Shot.Play();
        yield return new WaitForSeconds(delay);
        if (!IsPlaying || Shot._audioSource.clip.name == "ShortSilence")
        {
            PlayHard(audioClip);
        }
    }

    private IEnumerator PlayAfterFade(Clip clip, float fadeOut)
    {
        yield return StopRoutine(fadeOut);
        PlayInternal(clip, false);
    }

    private IEnumerator StopRoutine(float fadeOut)
    {
        IsFading = true;
        try
        {
            yield return Shot._audioSource.FadeOut(fadeOut);
            Stop();
        }
        finally
        {
            IsFading = false;
        }
    }

    private bool Disabled(SituationType situation)
    {
        return
            SettingsManager.Settings.ExplorationVanillaOnly
                && (SituationType.ExplorationNight | SituationType.ExplorationDay 
                                                   | SituationType.ExplorationAurora).HasFlag(situation)
            || SettingsManager.Settings.WeatherVanillaOnly
                && (SituationType.WeatherBlizzard | SituationType.WeatherClear 
                                                  | SituationType.WeatherFog 
                                                  | SituationType.WeatherSnow).HasFlag(situation)
            || SettingsManager.Settings.TimeVanillaOnly
                && (SituationType.TimeDawn | SituationType.TimeDusk).HasFlag(situation)
            || SettingsManager.Settings.StalkedVanillaOnly
                && (SituationType.Stalked).HasFlag(situation)
            || SettingsManager.Settings.TimberwolfVanillaOnly
                && (SituationType.Timberwolf).HasFlag(situation)
            || SettingsManager.Settings.ConditionVanillaOnly
                && (SituationType.ConditionSuccess | SituationType.ConditionSorrow).HasFlag(situation);
    }

    private Clip ChooseRandomSoundtrack(ICollection<SoundtrackInfo> soundtracks)
    {
        var vanillaChance = SettingsManager.Settings.ModVanillaMusicChance;
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
                    LastSoundtrack = soundtrack;
                    return Main.PlaylistManager.GetClip(soundtrack);
                }
            }
        }
        
        return null;
    }

    private LocationType GetLocationType(string scene)
    {
        if (scene.EndsWith("Region"))
        {
            return LocationType.Region;
        }

        if (scene.Contains("TransitionZone"))
        {
            return LocationType.TransitionZone;
        }

        if (scene.Contains("Cave"))
        {
            return LocationType.Cave;
        }

        if (scene.Contains("Mine"))
        {
            return LocationType.Mine;
        }

        return LocationType.Building;
    }
}