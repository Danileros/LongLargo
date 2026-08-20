using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using LongLargo.Extensions;
using LongLargo.Managers;
using LongLargo.Utils;
using MelonLoader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LongLargo.Handlers;

[RegisterTypeInIl2Cpp]
public class SceneMusicManagerHandler : MonoBehaviour
{
    private SceneMusicManager _instance;
    private float _delayRange;
    private float _minimalDelay = 60f;

    public SceneMusicManagerHandler(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<SceneMusicManager>();
        _delayRange = _instance.m_MaxSecondsBetweenExploreMusic - _instance.m_MinSecondsBetweenExploreMusic;
        if (!SettingsManager.Settings.ModEnabled)
        {
            return;
        }

        LLogger.Debug("[SceneMusicManager] awakens");
        var delayModifier = SettingsManager.Settings.ExplorationDelay;
        if (delayModifier != 100)
        {
            var modifier = delayModifier / 100f;
            _delayRange = (float)Math.Round(_delayRange * modifier);
            
            // Only max is changed because reset occurs when clip is started
            _instance.m_MaxSecondsBetweenExploreMusic = _instance.m_MinSecondsBetweenExploreMusic + _delayRange;
            _instance.m_TimeToPlayNextExploreMusic
                = Time.time + _instance.m_MinSecondsBetweenExploreMusic * modifier + Random.Range(0, _delayRange);
        }
        
        Main.DebugManager.RegisterDebugCommand("ll_debug_timer", DebugOutput);
    }

    public void OnDestroy()
    {
        Main.DebugManager.UnregisterDebugCommand("ll_debug_timer");
        LLogger.Debug("[SceneMusicManager] destroyed");
        // Main.AudioPlayer.Stop(1f);
    }

    /// <summary>
    /// Decides whatever we play custom exploration clip and which exactly.
    /// </summary>
    /// <returns>false if we should suppress original music.</returns>
    [HideFromIl2Cpp]
    public bool PlayExploreMusic()
    {
        if (Main.IsModDisabled())
        {
            return true;
        }

        if (SettingsManager.Settings.ExplorationSuppress)
        {
            _instance.ResetExploreMusicTimer();
            return false;
        }

        if (Main.AudioPlayer.IsPlaying)
        {
            return false;
        }
        
        var situation = SituationTypeExtensions.GetExplorationSituation();
        (var soundtrack, var allowVanilla) = Main.PlaylistManager.GetExplorationSoundtrack(situation);
        
        LLogger.Debug($"[SceneMusicManager] Choosing clip {soundtrack?.TrackName ?? "LongSilence"}");
        
        Main.AudioPlayer.PlaySoft(soundtrack, situation);
        if (!allowVanilla)
        {
            //_instance.ResetExploreMusicTimer();
            // Player can add LONG track so we should take it's length into equation instead of vanilla's fixed 240  
            var duration = (soundtrack?.Clip?.clipLength ?? 0f) < 60 ? 60f : (float)soundtrack.Clip.clipLength;
            _instance.m_TimeToPlayNextExploreMusic
                = Time.time + duration + _minimalDelay + Random.Range(0, _delayRange);
        }
        
        return allowVanilla;
    }

    [HideFromIl2Cpp]
    public string DebugOutput()
    {
        return $"Next at: {_instance.m_TimeToPlayNextExploreMusic - Time.time}";
    }
}