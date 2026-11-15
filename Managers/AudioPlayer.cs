using System.Collections;
using Il2Cpp;
using LongLargo.Extensions;
using LongLargo.Interfaces;
using LongLargo.Models;
using MelonLoader;
using UnityEngine;

namespace LongLargo.Managers;

public class AudioPlayer : IAudioPlayer
{
    private float LastTime = Time.time;

    public FSituationType LastSituation { get; private set; } = FSituationType.Disabled;

    public SoundtrackInfo LastSoundtrack { get; private set; }

    public float LastDelay { get; set; }

    public bool IsFading { get; private set; }

    public bool IsPaused => !IsPlaying && (_audioSource == null || _audioSource.time > 0f);

    public bool IsPlaying => _audioSource?.isPlaying ?? false;
    
    private static AudioSource _audioSource;
    private object _lastPlayToken;
    private object _lastStopToken;
    private GameObject _masterObject;

    public AudioPlayer()
    {
        Main.DebugManager.RegisterDebugCommand("ll_debug_audio", DebugData);
        _masterObject = new GameObject("LL_AudioPlayer");
        UnityEngine.GameObject.DontDestroyOnLoad(_masterObject);
        _audioSource = _masterObject.AddComponent<AudioSource>();
    }

    public void ResetVolume()
    {
        float masterVolume;
        float musicVolume;
        try
        {
            masterVolume = InterfaceManager.GetPanel<Panel_OptionsMenu>().State.m_MasterVolume;
            musicVolume = InterfaceManager.GetPanel<Panel_OptionsMenu>().State.m_MusicVolume;
        }
        catch (Exception)
        {
            masterVolume = musicVolume = 1;
        }
        
        if (SettingsManager.Settings.BgmVolumeEnabled)
        {
            SetVolume(masterVolume * SettingsManager.Settings.BgmVolume / 100f);
        }
        else
        {
            SetVolume(masterVolume * musicVolume);
        }
    }

    public void ResetLastSoundtrack()
    {
        LastSoundtrack = null;
    }

    public void SetVolume(float volume)
    {
        _audioSource.volume = volume;
    }
    
    public void PlaySoft(SoundtrackInfo soundtrack, FSituationType situation, bool loop = false)
    {
        if (soundtrack != null && !IsPlaying)
        {
            Stop();
            Main.Logger.Log($"[AudioPlayer] Now playing: {soundtrack.TrackName}");
            PlayInternal(soundtrack, situation, loop);
        }
    }

    public void PlaySoftDelayed(SoundtrackInfo soundtrack, FSituationType situation, float delay)
    {
        if (soundtrack != null && !IsPlaying)
        {
            Stop();
            Main.Logger.Log($"[AudioPlayer] Scheduled after {delay}: {soundtrack.TrackName}");
            
            PlayInternal(soundtrack, situation, false, delay);
        }
    }

    public void PlayHard(SoundtrackInfo soundtrack, FSituationType situation, bool loop = false)
    {
        if (soundtrack != null)
        {
            Stop();
            Main.Logger.Log($"[AudioPlayer] Now playing hard: {soundtrack.TrackName}");
            PlayInternal(soundtrack, situation, loop);
        }
    }

    public void PlayHard(SoundtrackInfo soundtrack, FSituationType situation, float fadeOut)
    {
        if (soundtrack != null)
        {
            Main.Logger.Log($"[AudioPlayer] Now playing hard with fade previous: {soundtrack.TrackName}");
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

    public void StopVanilla(GameObject go = null)
    {
        // <ActionPostEventEntry Id="2904596785" Name="musicMixer_StopSceneMusicFadeOut"/>
        // <ActionPostEventEntry Id="3267898453" Name="musicMixer_StopWeatherMusicFadeOut"/>
        GameAudioManager.PlaySound(2904596785U, go);
        GameAudioManager.PlaySound(3267898453U, go);
    }

    public void Stop()
    {
        Main.Logger.Debug("[AudioPlayer] Stopping");
        Main.Logger.Debug(new System.Diagnostics.StackTrace(true).ToString());
        
        _audioSource.Stop();
        _audioSource.loop = false;
        StopAllCoroutines();
    }

    public void Stop(float fadeOut)
    {
        if (!IsPlaying)
        {
            Main.Logger.Debug($"[AudioPlayer] Stop rejected, nothing to stop");
            return;
        }
        
        if (!IsFading)
        {
            Main.Logger.Debug($"[AudioPlayer] Stopping with fade out {fadeOut:N}");
            Main.Logger.Debug(new System.Diagnostics.StackTrace(true).ToString());
            
            _lastStopToken = MelonCoroutines.Start(this.StopRoutine(fadeOut));
        }
        else
        {
            Main.Logger.Debug($"[AudioPlayer] Stop rejected, already in process");
        }
    }

    public void StopIfSituation(FSituationType situations, float fadeOut = 0)
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
        
        Main.Logger.Debug($"[AudioPlayer] Stop rejected for {situations}, current is {LastSituation}");
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
            _audioSource.Pause();
        }
    }

    public void Resume()
    {
        if (IsPaused)
        {
            _audioSource.UnPause();
        }
    }

    public string DebugData()
    {
        var time = (int)_audioSource.time;
        var duration = (int)(LastSoundtrack?.Clip?.length ?? 0);
        var playtime = IsPlaying ? $"{time/60:00}:{time%60:00}/{duration/60:00}:{duration%60:00}" : "N/A";
        string delay = "N/A";
        if (IsPlaying && time <= 0)
        {
            delay = $"{Time.time - LastTime:F1}/{LastDelay}";
        }

        return $"IsPlaying:  {IsPlaying}, IsFading: {IsFading}, IsPaused: {IsPaused}\n" +
               $"Situation:  {LastSituation}, Looped: {_audioSource.loop}\n" +
               $"Soundtrack: {LastSoundtrack?.TrackName ?? "none"}\n" +
               $"Delay:      {delay}\n" +
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

    private void PlayInternal(SoundtrackInfo soundtrack, FSituationType situation, bool loop, float delay = 0.0f)
    {
        var clip = soundtrack.Clip;
        LastSoundtrack = soundtrack;
        LastSituation = situation;
        LastTime = Time.time;
        LastDelay = delay;
        _audioSource.Stop();
        ResetVolume();
        _audioSource.clip = clip;
        
        uConsole.Log($"[LL] Now playing: {soundtrack.TrackName}");
        
        _audioSource.loop = loop;
        if (delay <= 0.001f)
        {
            _audioSource.Play();
            return;
        }
        
        _audioSource.PlayScheduled(AudioSettings.dspTime + delay);
    }

    private IEnumerator PlayAfterFade(SoundtrackInfo soundtrack, FSituationType situation, float fadeOut)
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
        var startVolume = _audioSource.volume;
        var startTime = Time.time;
        var time = 0f;

        // sounds a bit better than linear
        while (time < fadeTime)
        {
            time = Time.time - startTime;
            SetVolume(startVolume * (float)(Math.Cos(time / fadeTime * Math.PI) + 1) / 2.0f);
            yield return null;
        }

        _audioSource.Stop();
        ResetVolume();
    }
}