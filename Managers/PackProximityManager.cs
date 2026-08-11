using System.Text;
using Il2Cpp;
using LongLargo.Interfaces;
using LongLargo.Models;
using LongLargo.Utils;
using UnityEngine;

namespace LongLargo.Managers;

public class PackProximityManager : IPackProximityManager
{
    
    private float nextUpdate = 0;
    private SoundtrackInfo _soundtrack;
    private GameObject _go;
    private readonly List<PackProximitySettings> _proximitySettings;
    private PackProximitySettings _settings;
    private float _distance;

    public bool IsInCombat { get; private set; } = false;
    public float FadeoutTimer { get; private set; } = 0;
    public PackProximityRange Range => _settings.PackProximityRange;

    public PackProximityManager(PackProximityRange settingsType)
    {
        var loader = new PackProximitySettingsLoader();
        _proximitySettings = loader.LoadAll();
        _settings = _proximitySettings.Single(s => s.PackProximityRange == settingsType);
        Main.DebugManager.RegisterDebugCommand("ll_debug_proximity", DebugData);
    }

    public void SelectSettings(PackProximityRange settingsType)
    {
        _settings = _proximitySettings.Single(s => s.PackProximityRange == settingsType);
        LLogger.Log($"Proximity settings selected: {settingsType}");
    }

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
        
        _distance = dinstance;;

        FadeoutTimer += Time.deltaTime;
        if (Main.AudioPlayer.IsPlaying
            && !Main.AudioPlayer.IsFading
            && Main.AudioPlayer.LastSituation == SituationType.Timberwolf)
        {
            if (dinstance > _settings.DistanceFadeInstant)
            {
                // Too far
                Main.AudioPlayer.Stop(3f);
            }
            else if (dinstance < _settings.DistanceCombat && !IsInSafety())
            {
                // Consider it to be an intense combat, no fadeout!
                FadeoutTimer = 0;
            }
            else if(FadeoutTimer > _settings.TimeNotInCombatBeforeFade)
            {
                // Not in combat range for too long, consider we're on safe elevation
                Main.AudioPlayer.Stop(3f);
            }
        }
        else
        {
            if (dinstance < _settings.DistanceCombat)
            {
                // In combat proximity again
                Main.AudioPlayer.PlayHard(_soundtrack, SituationType.Timberwolf, true);
                GameAudioManager.PlaySound(2904596785U, _go);
                GameAudioManager.PlaySound(3267898453U, _go);
                FadeoutTimer = 0;
            }
        }
    }

    private string DebugData()
    {
        return $"IsInCombat: {IsInCombat}/{GameManager.m_IsPaused}/{GameManager.s_IsGameplaySuspended}/{GameManager.s_IsAISuspended}\n"
               + $"Distance: {_distance:F2}, Range mode: {Range}\n"
               + $"Fadeout timer: {FadeoutTimer}\n"
               + $"IsInSafety: {IsInSafety()}";
    }

    private bool IsInSafety()
    {
        // is indoor that is not a cave or in a vehicle 
        return GameManager.GetPlayerManagerComponent().InHibernationPreventionIndoorEnvironment()
               || GameManager.GetPlayerInVehicle().IsInside();
    }
}