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
    private object _lastPlayToken;
    private object _lastFadeToken;
    private object _loopToken;

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

    public void ResetLastSoundtrack()
    {
        LastSoundtrack = null;
    }

    public void SetVolume(float volume)
    {
        Shot.SetVolume(volume);
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
            _lastPlayToken = MelonCoroutines.Start(this.PlayDelayedRoutine(soundtrack, situation, delay));
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
                PlayInternal(soundtrack, situation, false);
            }
            else
            {
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
        
        if (_loopToken != null)
        {
            MelonCoroutines.Stop(_loopToken);
        }
    }

    public void Stop(float fadeOut)
    {
        if (_loopToken != null)
        {
            MelonCoroutines.Stop(_loopToken);
        }
        
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
        if (situations.HasFlag(LastSituation))
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

    private void PlayInternal(SoundtrackInfo soundtrack, SituationType situation, bool loop)
    {
        var clip = soundtrack.Clip;
        LastSoundtrack = soundtrack;
        LastSituation = situation;
        Shot.AssignClip(clip);
        if (soundtrack == Main.PlaylistManager.LongSilence || soundtrack == Main.PlaylistManager.ShortSilence)
        {
            Shot.Play(); // no need to prefetch
            return;
        }

        Shot.Play(clip);
        if (loop)
        {
            _loopToken = MelonCoroutines.Start(PlayNextIteration());
        }
    }

    private IEnumerator PlayNextIteration()
    {
        while (IsPlaying || IsPaused)
        {
            yield return null;
        }
        
        PlayInternal(LastSoundtrack, LastSituation, true);
    }

    private IEnumerator PlayDelayedRoutine(SoundtrackInfo soundtrack, SituationType situation, float delay, bool loop = false)
    {
        Shot.AssignClip(Main.PlaylistManager.ShortSilence.Clip);
        Shot.Play();
        yield return new WaitForSeconds(delay);
        if (!IsPlaying || Shot._audioSource.clip.name == "ShortSilence")
        {
            PlayInternal(soundtrack, situation, loop);
        }
    }

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
            yield return Shot._audioSource.FadeOut(fadeOut);
            Stop();
        }
        finally
        {
            IsFading = false;
        }
    }
}