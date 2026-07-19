using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using LongLargo.Handlers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LongLargo.PatchImplementations;

[RegisterTypeInIl2Cpp]
public class SceneMusicManagerImpl : MonoBehaviour
{
    private SceneMusicManager _instance;
    private float _delayRange;
    private float _minimalDelay = 60f;

    public SceneMusicManagerImpl(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<SceneMusicManager>();
        _delayRange = _instance.m_MaxSecondsBetweenExploreMusic - _instance.m_MinSecondsBetweenExploreMusic;
        if (!LLSettings.settings.ModEnabled)
        {
            return;
        }

        LLogger.Debug("[SceneMusicManagerImpl] awakens");
        var delayModifier = LLSettings.settings.ExplorationDelay;
        if (delayModifier != 100)
        {
            var modifier = delayModifier / 100f;
            _delayRange = (float)Math.Round(_delayRange * modifier);
            
            // Only max is changed because reset occurs when clip is started
            _instance.m_MaxSecondsBetweenExploreMusic = _instance.m_MinSecondsBetweenExploreMusic + _delayRange;
            _instance.m_TimeToPlayNextExploreMusic
                = Time.time + _instance.m_MinSecondsBetweenExploreMusic * modifier + Random.Range(0, _delayRange);
        }
    }

    public void OnDestroy()
    {
        LLogger.Debug("[SceneMusicManagerImpl] destroyed");
        try
        {
            LongLargoMain.QueueManager.Stop(1f);
        }
        catch (Exception)
        {
            // Appears when exiting the game, no reason to worry
        }
    }

    /// <summary>
    /// Decides whatever we play custom exploration clip and which exactly.
    /// </summary>
    /// <returns>false if we should suppress original music.</returns>
    [HideFromIl2Cpp]
    public bool PlayExploreMusic()
    {
        if (ShouldSkip())
        {
            return true;
        }

        if (LLSettings.settings.ExplorationSuppress)
        {
            _instance.ResetExploreMusicTimer();
            return false;
        }

        if (LongLargoMain.QueueManager.IsPlaying)
        {
            return false;
        }
        
        LLogger.Debug("[SceneMusicManagerImpl] playing explore music");
        
        (var clip, var allowVanilla) = LongLargoMain.QueueManager.GetExplorationClip();
        
        LLogger.Debug($"[SceneMusicManagerImpl] chosen clip {clip?.audioClip?.name ?? "LongSilence"}");
        
        LongLargoMain.QueueManager.PlaySoft(clip);
        if (!allowVanilla)
        {
            //_instance.ResetExploreMusicTimer();
            // Player can add LONG track so we should take it's length into equation instead of vanilla's fixed 240  
            var duration = (clip?.clipLength ?? 0f) < 60 ? 60f : (float)clip.clipLength;
            _instance.m_TimeToPlayNextExploreMusic
                = Time.time + duration + _minimalDelay + Random.Range(0, _delayRange);
        }
        
        return allowVanilla;
    }

    private static bool ShouldSkip()
    {
        if (!LLSettings.settings.ModEnabled)
        {
            return true;
        }

        var scene = GameManager.m_ActiveScene;
        if (scene == null || scene.Contains("Menu") || scene.Contains("Boot")
            || scene.Contains("Bunker") || scene == "MiningRegionMine") // Don't mess with tales
        {
            return true;
        }

        return false;
    }
}