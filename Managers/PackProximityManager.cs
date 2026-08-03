using System.Text;
using Il2Cpp;
using LongLargo.Interfaces;
using LongLargo.Model;
using UnityEngine;

namespace LongLargo.Managers;

public class PackProximityManager : IPackProximityManager
{
    private const float TimeNotInCombatBeforeFade = 60;
    private const float DistanceCombat = 50;
    private const float DistanceFadeInstant = 150;

    private float nextUpdate = 0;
    private SoundtrackInfo _soundtrack;
    private GameObject _go;
    
    public bool IsInCombat { get; private set; } = false;
    public float FadeoutTimer { get; private set; } = 0;

    /// <summary>
    /// Executes on Play_TimberwolfCombat event, pack morale hud activates.
    /// </summary>
    public SituationType OnPlayCombat(GameObject go)
    {
        _go = go;
        if (!SettingsManager.Settings.TimberwolfSuppress)
        {
            (_soundtrack, _) = Main.PlaylistManager.GetSituationSoundtrack(SituationType.Timberwolf, true);
            Main.AudioPlayer.PlayHard(_soundtrack, SituationType.Timberwolf, true);
            GameAudioManager.PlaySound(2904596785U, go);
            GameAudioManager.PlaySound(3267898453U, go);
        }

        IsInCombat = true;
        FadeoutTimer = 0;
        return SituationType.Disabled;
    }

    /// <summary>
    /// Executes on Stop_TimberwolfCombat event or scene load, pack morale hud deactivates.
    /// </summary>
    public SituationType OnStopCombat()
    {
        if (!IsInCombat)
        {
            return SituationType.Disabled;
        }
        
        IsInCombat = false;
        if (Main.AudioPlayer.IsPlaying && Main.AudioPlayer.LastSituation == SituationType.Timberwolf)
        {
            if (!string.IsNullOrEmpty(_soundtrack.StopTrackName))
            {
                var stopTrack = Main.PlaylistManager.Soundtracks
                    .FirstOrDefault(t => t.AssetBundle == _soundtrack.AssetBundle
                                         && t.TrackName == _soundtrack.StopTrackName);

                Main.AudioPlayer.PlayHard(stopTrack, SituationType.Disabled);
            }
            else
            {
                Main.AudioPlayer.Stop(3f);
            }
        }
        
        return SituationType.Disabled;
    }

    /// <summary>
    /// Executes on scene change.
    /// </summary>
    public void ForceLeaveCombat()
    {
        IsInCombat = false;
        Main.AudioPlayer.StopIfSituation(SituationType.Timberwolf, 1f);
    }

    /// <summary>
    /// Distance-based music play.
    /// </summary>
    public void UpdateMusic(float dinstance)
    {
        if (!IsInCombat || SettingsManager.Settings.TimberwolfSuppress
                        || GameManager.m_IsPaused
                        || GameManager.s_IsGameplaySuspended
                        || GameManager.s_IsAISuspended)
        {
            return;
        }

        FadeoutTimer += Time.deltaTime;
        if (Main.AudioPlayer.IsPlaying
            && !Main.AudioPlayer.IsFading
            && Main.AudioPlayer.LastSituation == SituationType.Timberwolf)
        {
            if (dinstance > DistanceFadeInstant)
            {
                // Too far
                Main.AudioPlayer.Stop(3f);
            }
            else if (dinstance < DistanceCombat)
            {
                // Consider it to be an intense combat, no fadeout!
                FadeoutTimer = 0;
            }
            else if(FadeoutTimer > TimeNotInCombatBeforeFade)
            {
                // Not in combat range for too long, consider we're on safe elevation
                Main.AudioPlayer.Stop(3f);
            }
        }
        else
        {
            if (dinstance < DistanceCombat)
            {
                // In combat proximity again
                Main.AudioPlayer.PlayHard(_soundtrack, SituationType.Timberwolf, true);
                GameAudioManager.PlaySound(2904596785U, _go);
                GameAudioManager.PlaySound(3267898453U, _go);
                FadeoutTimer = 0;
            }
        }
    }
}