using System.Text;
using Il2Cpp;
using LongLargo.Interfaces;
using LongLargo.Models;
using LongLargo.Utils;
using UnityEngine;

namespace LongLargo.Managers;

public class PackCombatManager : IPackCombatManager
{
    
    private float nextUpdate = 0;
    private SoundtrackInfo _soundtrack;
    private GameObject _go;
    private readonly List<PackProximitySettings> _proximitySettings;
    private PackProximitySettings _settings;
    private float _distance;
    private float _lastMoraleChange;

    public bool IsInCombat { get; private set; } = false;
    public float FadeoutTimer { get; private set; } = 0;
    public EPackProximityRange Range => _settings.EPackProximityRange;

    public PackCombatManager(EPackProximityRange settingsType)
    {
        var loader = new PackProximitySettingsLoader();
        _proximitySettings = loader.LoadAll();
        _settings = _proximitySettings.Single(s => s.EPackProximityRange == settingsType);
        Main.DebugManager.RegisterDebugCommand("ll_debug_combat", DebugData);
    }

    public void SelectSettings(EPackProximityRange settingsType)
    {
        _settings = _proximitySettings.Single(s => s.EPackProximityRange == settingsType);
        LLogger.Log($"Proximity settings selected: {settingsType}");
    }

    /// <summary>
    /// Executes on Play_TimberwolfCombat event, pack morale hud activates.
    /// </summary>
    public FSituationType OnPlayCombat(GameObject go)
    {
        _go = go;
        if (!SettingsManager.Settings.TimberwolfSuppress)
        {
            (_soundtrack, _) = Main.PlaylistManager.GetSituationSoundtrack(FSituationType.Timberwolf, true);
            Main.AudioPlayer.PlayHard(_soundtrack, FSituationType.Timberwolf, true);
            Main.AudioPlayer.StopVanilla();
        }

        IsInCombat = true;
        FadeoutTimer = 0;
        return FSituationType.Disabled;
    }

    /// <summary>
    /// Executes on Stop_TimberwolfCombat event or scene load, pack morale hud deactivates.
    /// </summary>
    public FSituationType OnStopCombat()
    {
        if (!IsInCombat)
        {
            return FSituationType.Disabled;
        }
        
        IsInCombat = false;
        if (Main.AudioPlayer.IsPlaying && Main.AudioPlayer.LastSituation == FSituationType.Timberwolf)
        {
            if (!string.IsNullOrEmpty(_soundtrack.StopTrackName))
            {
                var stopTrack = Main.PlaylistManager.Soundtracks
                    .FirstOrDefault(t => t.AssetBundle == _soundtrack.AssetBundle
                                         && t.TrackName == _soundtrack.StopTrackName);

                Main.AudioPlayer.PlayHard(stopTrack, FSituationType.Disabled);
            }
            else
            {
                Main.AudioPlayer.Stop(3f);
            }
        }
        
        return FSituationType.Disabled;
    }

    /// <summary>
    /// Executes on scene change.
    /// </summary>
    public void ForceLeaveCombat()
    {
        IsInCombat = false;
        Main.AudioPlayer.StopIfSituation(FSituationType.Timberwolf, 1f);
    }

    /// <summary>
    /// Distance-based music play.
    /// </summary>
    public void UpdateMusic(float dinstance, bool moraleChanged)
    {
        if (!IsInCombat || SettingsManager.Settings.TimberwolfSuppress
                        || GameManager.m_IsPaused
                        || GameManager.s_IsGameplaySuspended
                        || GameManager.s_IsAISuspended)
        {
            return;
        }
        
        _distance = dinstance;
        
        if (moraleChanged)
        {
            _lastMoraleChange = Time.time;
        }

        FadeoutTimer += Time.deltaTime;

        bool IsActiveCombat()
        {
            // Every morale lowering is an active combat
            // Also, being close and not in safety is an active combat
            return (dinstance < _settings.DistanceCombat && !IsInSafety()) || moraleChanged;
        }

        if (Main.AudioPlayer.IsPlaying
            && !Main.AudioPlayer.IsFading
            && Main.AudioPlayer.LastSituation == FSituationType.Timberwolf)
        {
            if (dinstance > _settings.DistanceFadeInstant)
            {
                // Too far
                Main.AudioPlayer.Stop(3f);
            }
            else if (IsActiveCombat())
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
            if (IsActiveCombat())
            {
                // In intense combat again
                Main.AudioPlayer.PlayHard(_soundtrack, FSituationType.Timberwolf, true);
                Main.AudioPlayer.StopVanilla(_go);
                FadeoutTimer = 0;
            }
        }
    }

    private string DebugData()
    {
        return $"IsInCombat: {IsInCombat}/{GameManager.m_IsPaused}/{GameManager.s_IsGameplaySuspended}/{GameManager.s_IsAISuspended}\n"
               + $"Distance: {_distance:F2}, Range mode: {Range}\n"
               + $"Fadeout timer: {FadeoutTimer:F2}, LastMoraleUpdate: {Time.time - _lastMoraleChange:F2}\n"
               + $"IsInSafety: {IsInSafety()}";
    }

    private bool IsInSafety()
    {
        return IsInSafeHouse() || IsInVehicle() || IsNearFire();
    }

    // Is indoor that is not a cave 
    private static bool IsInSafeHouse()
    {
        return GameManager.GetPlayerManagerComponent().InHibernationPreventionIndoorEnvironment();
    }

    private static bool IsInVehicle()
    {
        return GameManager.GetPlayerInVehicle().IsInside();
    }

    // Actually, fire gives you 15m protection circle against timberwolves.
    // It works for every fire stage, even while starting fire, you are already protected.
    // But only if campfire is visible for them. And I can't be sure it is visible from any direction.
    // And fire starting could fail. So only being really close to fully burning fire counts as safety.
    private bool IsNearFire()
    {
        return GameManager.GetPlayerTransform() != null
               && GameManager.GetFireManagerComponent()
                   .PointInRadiusOfFullyBurningFire(GameManager.GetPlayerTransform().position);
    }
}