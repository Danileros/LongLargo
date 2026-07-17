using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioMgr;
using Il2Cpp;
using Il2CppSystem.Linq;
using LongLargo.Extensions;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo.Handlers;

/// <summary>
/// Handles soundtracks queue. Chooses which track to play by itself.
/// </summary>
public class QueueManager
{
    private readonly SoundtrackInfo[] _soundtracks;
    private static Shot _shot;
    private bool isPaused = false;
    private object _lastToken;

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
                        AudioMaster.CreateMasterParent();
                        _shot = AudioMaster.CreatePlayerShot(AudioMaster.SourceType.BGM);
                    }
                }
            }
            
            return _shot;
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
    
    /// <summary>
    /// Tries to play and gently refuses if something is playing already.
    /// </summary>
    /// <param name="clip">Clip.</param>
    /// <param name="loop">true if it should be looped.</param>
    public void PlaySoft(Clip clip, bool loop = false)
    {
        isPaused = false;
        if (clip != null && !Shot._audioSource.isPlaying)
        {
            LLogger.Log($"Now playing: {clip.audioClip.name}");
            Shot._audioSource.loop = loop;
            Shot.AssignClip(clip);
            Shot.Play();
        }
    }
    

    /// <summary>
    /// Tries to play scheduled and gently refuses if something is playing already.
    /// </summary>
    /// <param name="clip">Clip.</param>
    /// <param name="loop">true if it should be looped.</param>
    public void PlaySoftDelayed(Clip clip, float delay)
    {
        isPaused = false;
        if (clip != null && !Shot._audioSource.isPlaying)
        {
            LLogger.Log($"Scheduled after {delay}: {clip.audioClip.name}");
            Shot._audioSource.loop = false;
            Shot.AssignClip(LongLargoMain.PlaylistProvider.ShortSilence);
            Shot.Play();
            _lastToken = MelonCoroutines.Start(this.PlayRoutine(clip, delay));
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
            LLogger.Log($"Now playing hard: {clip.audioClip.name}");
            Shot._audioSource.loop = loop;
            Shot.AssignClip(clip);
            Shot.Play();
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
            LLogger.Log($"Now playing hard with fade: {clip.audioClip.name}");
            if (!IsPlaying)
            {
                Shot.AssignClip(clip);
                Shot.Play();
            }
            else
            {
                _lastToken = MelonCoroutines.Start(this.PlayAfterFade(clip, fadeOut));
            }
        }
    }

    public void Stop()
    {
        isPaused = false;
        Shot._audioSource.loop = false;
        Shot.Stop();
        if (_lastToken != null)
        {
            MelonCoroutines.Stop(_lastToken);
        }
    }

    public void Stop(float fadeOut)
    {
        MelonCoroutines.Start(this.StopRoutine(fadeOut));
    }

    public void Pause()
    {
        if (LongLargoMain.QueueManager.IsPlaying)
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
        var timeOfDay = GameManager.GetTimeOfDayComponent();
        var aurora = GameManager.GetAuroraManager();
        var situation = aurora.AuroraIsActive() ? SituationType.ExplorationAurora : (timeOfDay.IsDay() ? SituationType.ExplorationDay : SituationType.ExplorationNight);
        if (Disabled(situation))
        {
            return (LongLargoMain.PlaylistProvider.LongSilence, true);
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
            return (LongLargoMain.PlaylistProvider.LongSilence, true);
        }
    }
    
    /// <summary>
    /// Gets random situation clip.
    /// </summary>
    /// <returns>(Clip, playVanilla)</returns>
    public (Clip, bool) GetSituationClip(SituationType situation)
    {
        if (Disabled(situation))
        {
            return (LongLargoMain.PlaylistProvider.ShortSilence, true);
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
            return (LongLargoMain.PlaylistProvider.ShortSilence, true);
        }
    }

    private IEnumerator PlayRoutine(Clip audioClip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayHard(audioClip);
    }

    private IEnumerator PlayAfterFade(Clip clip, float fadeOut)
    {
        yield return Shot._audioSource.FadeOut(fadeOut);
        Shot.AssignClip(clip);
        Shot.Play();
    }
    
    // private IEnumerator PlayRoutine(Clip audioClip, float delay)
    // {
    //      var time = AudioSettings.dspTime + delay;
    //      var _endTime = time + audioClip.clipLength + delay;
    //      Shot._audioSource.PlayDelayed(delay);
    //      Shot.SetFieldValue("_playState", Shot.PlayState.Playing);
    //      while (AudioSettings.dspTime < _endTime)
    //          yield return (object) null;
    //      this.Stop();
    // }

    private IEnumerator StopRoutine(float fadeOut)
    {
        yield return Shot._audioSource.FadeOut(fadeOut);
        isPaused = false;
        Shot._audioSource.loop = false;
        Shot.Stop();
    }

    private bool Disabled(SituationType situation)
    {
        return
            LLSettings.settings.ExplorationVanillaOnly
                && (SituationType.ExplorationNight | SituationType.ExplorationDay 
                                                   | SituationType.ExplorationAurora).HasFlag(situation)
            || LLSettings.settings.WeatherVanillaOnly
                && (SituationType.WeatherBlizzard | SituationType.WeatherClear 
                                                  | SituationType.WeatherFog 
                                                  | SituationType.WeatherSnow).HasFlag(situation)
            || LLSettings.settings.TimeVanillaOnly
                && (SituationType.TimeDawn | SituationType.TimeDusk).HasFlag(situation)
            || LLSettings.settings.StalkedVanillaOnly
                && (SituationType.Stalked).HasFlag(situation)
            || LLSettings.settings.TimberwolfVanillaOnly
                && (SituationType.Timberwolf).HasFlag(situation)
            || LLSettings.settings.SuccessVanillaOnly
                && (SituationType.Success | SituationType.Sorrow).HasFlag(situation);
    }

    private Clip ChooseRandomSoundtrack(ICollection<SoundtrackInfo> soundtracks)
    {
        var vanillaChance = LLSettings.settings.ModVanillaMusicChance;
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
                    return LongLargoMain.PlaylistProvider.GetClip(soundtrack);
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