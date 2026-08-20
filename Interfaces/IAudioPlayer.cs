using LongLargo.Models;

namespace LongLargo.Interfaces;

/// <summary>
/// Responsible for playing soundtrack. An adapter for AudioManager's Shot class. 
/// </summary>
public interface IAudioPlayer
{
    /// <summary>
    /// Situation that triggered current (or last) soundtrack. Always one flag.
    /// </summary>
    SituationType LastSituation { get; }
    
    /// <summary>
    /// Last (or current) soundtrack. Can be null.
    /// </summary>
    SoundtrackInfo LastSoundtrack { get; }
    
    /// <summary>
    /// Only when IsPlaying = true. Volume is decreasing. Soon Stop() will trigger and sound will reset. 
    /// </summary>
    bool IsFading { get; }
    
    /// <summary>
    /// Only when IsPlaying = false.
    /// </summary>
    bool IsPaused { get; }
    
    /// <summary>
    /// true when anything is playing. Includes delayed track and IsFading.
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// Forget last soundtrack (to enable playing same track again).
    /// </summary>
    void ResetLastSoundtrack();
    
    /// <summary>
    /// Changes volume.
    /// </summary>
    void SetVolume(float volume);

    /// <summary>
    /// Tries to play and gently refuses if something is playing already.
    /// </summary>
    /// <param name="soundtrack">Soundtrack.</param>
    /// <param name="situation">Situation.</param>
    /// <param name="loop">true if it should be looped.</param>
    void PlaySoft(SoundtrackInfo soundtrack, SituationType situation, bool loop = false);

    /// <summary>
    /// Tries to play with delay and gently refuses if something is playing already.
    /// </summary>
    /// <param name="soundtrack">Soundtrack.</param>
    /// <param name="situation">Situation.</param>
    /// <param name="delay">Delay to start.</param>
    void PlaySoftDelayed(SoundtrackInfo soundtrack, SituationType situation, float delay);

    /// <summary>
    /// Stops whatever we play already and plays.
    /// </summary>
    /// <param name="soundtrack">Soundtrack.</param>
    /// <param name="situation">Situation.</param>
    /// <param name="loop">true if it should be looped.</param>
    void PlayHard(SoundtrackInfo soundtrack, SituationType situation, bool loop = false);

    /// <summary>
    /// Stops whatever we play already and plays.
    /// </summary>
    /// <param name="soundtrack">Soundtrack.</param>
    /// <param name="situation">Situation.</param>
    /// <param name="fadeOut">time to fade out current track (if any).</param>
    void PlayHard(SoundtrackInfo soundtrack, SituationType situation, float fadeOut);

    /// <summary>
    /// Stops current track and all routines.
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Stops current track with fadeout and all routines.
    /// </summary>
    void Stop(float fadeOut);

    /// <summary>
    /// Stops current track if it matches any situation from input. Input like ExplorationDay | ExplorationNight.
    /// </summary>
    void StopIfSituation(SituationType situations, float fadeOut = 0);

    /// <summary>
    /// Break the Silence.
    /// </summary>
    void StopIfSilence();
    
    /// <summary>
    /// Pauses.
    /// </summary>
    void Pause();
    
    /// <summary>
    /// Unpauses.
    /// </summary>
    void Resume();

    /// <summary>
    /// Debug output.
    /// </summary>
    /// <returns>Debug text.</returns>
    string DebugData();
}