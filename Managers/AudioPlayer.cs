using System.Collections;
using AudioMgr;
using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Interfaces;
using LongLargo.Model;
using LongLargo.Utils;
using MelonLoader;
using UnityEngine;

namespace LongLargo.Managers;

public class AudioPlayer : IAudioPlayer
{
    private float LastTime = Time.time;
        
    public SituationType LastSituation { get; private set; } = SituationType.Disabled;

    public SoundtrackInfo LastSoundtrack { get; private set; }

    public bool IsFading { get; private set; }

    public bool IsPaused => !IsPlaying && Shot._audioSource.time > 0f;

    public bool IsPlaying => Shot._audioSource.isPlaying;

    private static Shot Shot
    {
        get
        {
            if (!_shot)
            {
                lock (typeof(AudioPlayer))
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
    
    private static Shot _shot;
    private static VolumeMaster.OnVolumeChange _onVolumeChange;
    private static float _masterVolume;
    private object _lastPlayToken;
    private object _lastFadeToken;
    // private object _loopToken;

    public static void ResetVolume()
    {
        if (SettingsManager.Settings.BgmVolumeEnabled)
        {
            try
            {
                _masterVolume = InterfaceManager.GetPanel<Panel_OptionsMenu>().State.m_MasterVolume;
            }
            catch (Exception)
            {
                // ignored
            }

            var masterVolume = _masterVolume;
            Shot._audioSource.volume = masterVolume * SettingsManager.Settings.BgmVolume / 100f;
        }
        else
        {
            Shot._audioSource.volume = VolumeMaster.GetVolume(AudioMaster.SourceType.BGM);
        }
    }

    public void ResetLastSoundtrack()
    {
        LastSoundtrack = null;
    }

    public void SetVolume(float volume)
    {
        Shot._audioSource.volume = volume;
    }
    
    public void PlaySoft(SoundtrackInfo soundtrack, SituationType situation, bool loop = false)
    {
        if (soundtrack != null && !IsPlaying)
        {
            Stop();
            LLogger.Log($"[Queue] Now playing: {soundtrack.TrackName}");
            PlayInternal(soundtrack, situation, loop);
        }
    }

    public void PlaySoftDelayed(SoundtrackInfo soundtrack, SituationType situation, float delay)
    {
        if (soundtrack != null && !IsPlaying)
        {
            Stop();
            LLogger.Log($"[Queue] Scheduled after {delay}: {soundtrack.TrackName}");
            
            //_lastPlayToken = MelonCoroutines.Start(this.PlayDelayedRoutine(soundtrack, situation, delay));
            PlayInternal(soundtrack, situation, false, delay);
        }
    }

    public void PlayHard(SoundtrackInfo soundtrack, SituationType situation, bool loop = false)
    {
        if (soundtrack != null)
        {
            Stop();
            LLogger.Log($"[Queue] Now playing hard: {soundtrack.TrackName}");
            PlayInternal(soundtrack, situation, loop);
        }
    }

    public void PlayHard(SoundtrackInfo soundtrack, SituationType situation, float fadeOut)
    {
        if (soundtrack != null)
        {
            LLogger.Log($"[Queue] Now playing hard with fade previous: {soundtrack.TrackName}");
            if (!IsPlaying)
            {
                Stop();
                PlayInternal(soundtrack, situation, false);
            }
            else
            {
                StopAllCoroutines();
                _lastPlayToken = MelonCoroutines.Start(this.PlayAfterFade(soundtrack, situation, fadeOut));
            }
        }
    }

    public void Stop()
    {
        if (SettingsManager.Settings.DebugMode)
        {
            LLogger.Debug("[Queue] Stopping");
            LLogger.Debug(new System.Diagnostics.StackTrace(true).ToString());
        }
        
        Shot._audioSource.Stop();
        Shot._audioSource.loop = false;
        StopAllCoroutines();
    }

    public void Stop(float fadeOut)
    {
        // if (_loopToken != null)
        // {
        //     MelonCoroutines.Stop(_loopToken);
        //     _loopToken = null;
        // }
        
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
        if (situations.HasFlagSafe(LastSituation))
        {
            if (!IsPlaying)
            {
                return;
            }

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
        if (Main.AudioPlayer.IsPlaying)
        {
            Shot._audioSource.Pause();
        }
    }

    public void Resume()
    {
        if (IsPaused)
        {
            Shot._audioSource.UnPause();
        }
    }

    public string DebugData()
    {
        var time = Shot._audioSource.time;
        var duration = LastSoundtrack?.Clip?.clipLength ?? 0;
        var playtime = IsPlaying ? $"{time/60:00}:{time%60:00}/{duration/60:00}:{duration%60:00}" : "N/A";
        return $"IsPlaying:  {IsPlaying}\n" +
               $"IsFading:   {IsFading}\n" +
               $"IsPaused:   {IsPaused}\n" +
               $"Looped:     {Shot._audioSource.loop}\n" +
               $"Situation:  {LastSituation}\n" +
               $"Soundtrack: {LastSoundtrack?.TrackName ?? "none"}\n" +
               $"Delay:      {Time.time - LastTime:F1}\n" +
               $"Time:       {playtime}";

    }

    private void StopAllCoroutines()
    {
        StopCoroutine(ref _lastPlayToken);
        // StopCoroutine(ref _loopToken);
        StopCoroutine(ref _lastFadeToken);
        IsFading = false;
        ResetVolume();
    }

    private void StopCoroutine(ref object token)
    {
        if (token != null)
        {
            MelonCoroutines.Stop(token);
            token = null;
        }
    }

    private void PlayInternal(SoundtrackInfo soundtrack, SituationType situation, bool loop, float delay = 0.0f)
    {
        ResetVolume();
        var clip = soundtrack.Clip;
        LastSoundtrack = soundtrack;
        LastSituation = situation;
        LastTime = Time.time;
        Shot._audioSource.Stop();
        Shot._audioSource.clip = clip.audioClip;
        
        uConsole.Log($"[LL] Now playing: {soundtrack.TrackName}");
        
        Shot._audioSource.loop = loop;
        if (delay <= 0.001f
            || soundtrack == Main.PlaylistManager.LongSilence
            || soundtrack == Main.PlaylistManager.ShortSilence)
        {
            Shot._audioSource.Play();
            return;
        }
        
        Shot._audioSource.PlayScheduled(AudioSettings.dspTime + delay);
        // if (loop)
        // {
        //     _loopToken = MelonCoroutines.Start(PlayNextIteration());
        // }
    }

    // private IEnumerator PlayNextIteration()
    // {
    //     while (IsPlaying || IsPaused)
    //     {
    //         yield return null;
    //     }
    //     
    //     PlayInternal(LastSoundtrack, LastSituation, true);
    // }

    // private IEnumerator PlayDelayedRoutine(SoundtrackInfo soundtrack, SituationType situation, float delay, bool loop = false)
    // {
    //     Shot._audioSource.Stop();
    //     Shot._audioSource.clip = Main.PlaylistManager.ShortSilence.Clip.audioClip;
    //     Shot._audioSource.Play();
    //     yield return new WaitForSeconds(delay);
    //     if (!IsPlaying || Shot._audioSource.clip.name == "ShortSilence")
    //     {
    //         PlayInternal(soundtrack, situation, loop);
    //     }
    // }

    private IEnumerator PlayAfterFade(SoundtrackInfo soundtrack, SituationType situation, float fadeOut)
    {
        yield return StopRoutine(fadeOut);
        PlayInternal(soundtrack, situation, false);
    }

    private IEnumerator StopRoutine(float fadeOut)
    {
        IsFading = true;
        try
        {
            yield return FadeOut(fadeOut);
            Stop();
        }
        finally
        {
            IsFading = false;
        }
    }
    
     /// <summary>
     /// Stops with fade.
     /// </summary>
     /// <param name="fadeTime">Time to fade sound completely.</param>
     /// <returns>Reason to use MelonCoroutines.</returns>
     public IEnumerator FadeOut(float fadeTime)
     {
         var startVolume = Shot._audioSource.volume;
         var startTime = Time.time;
         var time = 0f;
         
         // sounds a bit better than linear
         while (time < fadeTime)
         {
             time = Time.time - startTime;
             SetVolume(startVolume * (float)(Math.Cos(time / fadeTime * Math.PI) + 1) / 2.0f);
             yield return null;
         }
         
         //// linear
         // while (audioSource.volume > 0) {
         //     audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
         //     yield return null;
         // }
         
         Stop();
     }
}