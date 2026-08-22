using System.Collections;
using AudioMgr;
using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Interfaces;
using LongLargo.Models;
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
    private object _lastStopToken;

    public AudioPlayer()
    {
        Main.DebugManager.RegisterDebugCommand("ll_debug_audio", DebugData);
    }

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
            LLogger.Log($"[AudioPlayer] Now playing: {soundtrack.TrackName}");
            PlayInternal(soundtrack, situation, loop);
        }
    }

    public void PlaySoftDelayed(SoundtrackInfo soundtrack, SituationType situation, float delay)
    {
        if (soundtrack != null && !IsPlaying)
        {
            Stop();
            LLogger.Log($"[AudioPlayer] Scheduled after {delay}: {soundtrack.TrackName}");
            
            PlayInternal(soundtrack, situation, false, delay);
        }
    }

    public void PlayHard(SoundtrackInfo soundtrack, SituationType situation, bool loop = false)
    {
        if (soundtrack != null)
        {
            Stop();
            LLogger.Log($"[AudioPlayer] Now playing hard: {soundtrack.TrackName}");
            PlayInternal(soundtrack, situation, loop);
        }
    }

    public void PlayHard(SoundtrackInfo soundtrack, SituationType situation, float fadeOut)
    {
        if (soundtrack != null)
        {
            LLogger.Log($"[AudioPlayer] Now playing hard with fade previous: {soundtrack.TrackName}");
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
            LLogger.Debug("[AudioPlayer] Stopping");
            LLogger.Debug(new System.Diagnostics.StackTrace(true).ToString());
        }
        
        Shot._audioSource.Stop();
        Shot._audioSource.loop = false;
        StopAllCoroutines();
    }

    public void Stop(float fadeOut)
    {
        if (!IsPlaying)
        {
            LLogger.Debug($"[AudioPlayer] Stop rejected, nothing to stop");
            return;
        }
        
        if (!IsFading)
        {
            if (SettingsManager.Settings.DebugMode)
            {
                LLogger.Debug($"[AudioPlayer] Stopping with fade out {fadeOut:N}");
                LLogger.Debug(new System.Diagnostics.StackTrace(true).ToString());
            }
            
            _lastStopToken = MelonCoroutines.Start(this.StopRoutine(fadeOut));
        }
        else
        {
            LLogger.Debug($"[AudioPlayer] Stop rejected, already in process");
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
        
        LLogger.Debug($"[AudioPlayer] Stop rejected for {situations}, current is {LastSituation}");
    }

    public void StopIfSilence()
    {
        if (!IsPlaying)
        {
            return;
        }

        if (LastSoundtrack == Main.PlaylistManager.LongSilence
            || LastSoundtrack == Main.PlaylistManager.ShortSilence)
        {
            Stop();
        }
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
        var time = (int)Shot._audioSource.time;
        var duration = (int)(LastSoundtrack?.Clip?.clipLength ?? 0);
        var playtime = IsPlaying ? $"{time/60:00}:{time%60:00}/{duration/60:00}:{duration%60:00}" : "N/A";
        return $"IsPlaying: {IsPlaying}, IsFading: {IsFading}, IsPaused: {IsPaused}\n" +
               $"Situation:  {LastSituation}, Looped: {Shot._audioSource.loop}\n" +
               $"Soundtrack: {LastSoundtrack?.TrackName ?? "none"}\n" +
               $"Delay:      {Time.time - LastTime:F1}\n" +
               $"Time:       {playtime}";

    }

    private void StopAllCoroutines()
    {
        StopCoroutine(ref _lastPlayToken);
        StopCoroutine(ref _lastStopToken);
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
        var clip = soundtrack.Clip;
        LastSoundtrack = soundtrack;
        LastSituation = situation;
        LastTime = Time.time;
        Shot._audioSource.Stop();
        ResetVolume();
        Shot._audioSource.clip = clip.audioClip;
        
        uConsole.Log($"[LL] Now playing: {soundtrack.TrackName}");
        
        Shot._audioSource.loop = loop;
        if (delay <= 0.001f)
        {
            Shot._audioSource.Play();
            return;
        }
        
        Shot._audioSource.PlayScheduled(AudioSettings.dspTime + delay);
    }

    private IEnumerator PlayAfterFade(SoundtrackInfo soundtrack, SituationType situation, float fadeOut)
    {
        yield return FadeRoutine(fadeOut);
        PlayInternal(soundtrack, situation, false);
    }

    private IEnumerator StopRoutine(float fadeOut)
    {
        yield return FadeRoutine(fadeOut);
        Stop();
    }

    private IEnumerator FadeRoutine(float fadeOut)
    {
        IsFading = true;
        try
        {
            yield return FadeOut(fadeOut);
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

        Shot._audioSource.Stop();
        ResetVolume();
    }
}