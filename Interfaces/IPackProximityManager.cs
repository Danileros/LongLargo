using LongLargo.Models;
using UnityEngine;

namespace LongLargo.Interfaces;

/// <summary>
/// Special manager for distance-based Timberwolf combat music.
/// Just enabling music for combat is not enough - player can successfully escape from them or they could stuck.
/// Keeping combat music when player is 1 km away does not makes any sense.
/// </summary>
public interface IPackProximityManager
{
    /// <summary>
    /// Selects settings (when user changed it).
    /// </summary>
    /// <param name="settingsType">Selected setting.</param>
    void SelectSettings(PackProximityRange settingsType);
    
    /// <summary>
    /// In combat is when timberwolf combat bar is active. 
    /// </summary>
    bool IsInCombat { get; }
    
    /// <summary>
    /// Current settings for proximity.
    /// </summary>
    PackProximityRange Range { get; }
    
    /// <summary>
    /// When runs too high, music will fadeout. Only runs when game is not paused and player is far from wolves.
    /// </summary>
    float FadeoutTimer { get; }

    /// <summary>
    /// Executes on Play_TimberwolfCombat event, pack morale hud activates.
    /// </summary>
    SituationType OnPlayCombat(GameObject go);

    /// <summary>
    /// Executes on Stop_TimberwolfCombat event or scene load, pack morale hud deactivates.
    /// </summary>
    SituationType OnStopCombat();

    /// <summary>
    /// Executes on scene change.
    /// </summary>
    void ForceLeaveCombat();

    /// <summary>
    /// Distance-based music play.
    /// </summary>
    void UpdateMusic(float dinstance);
}