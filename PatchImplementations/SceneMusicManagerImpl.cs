using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using LongLargo.Handlers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo.PatchImplementations;

[RegisterTypeInIl2Cpp]
public class SceneMusicManagerImpl : MonoBehaviour
{
    private SceneMusicManager _instance;
    
    public SceneMusicManagerImpl(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<SceneMusicManager>();
        if (!LLSettings.settings.ModEnabled)
        {
            return;
        }

        LLogger.Debug("SceneMusicManagerImpl awakens");
        var delayModifier = LLSettings.settings.ExplorationDelay;
        if (delayModifier != 100)
        {
            var modifier = delayModifier / 100f;
            // Only max is changed
            var range = _instance.m_MaxSecondsBetweenExploreMusic - _instance.m_MinSecondsBetweenExploreMusic;
            _instance.m_MaxSecondsBetweenExploreMusic =
                _instance.m_MinSecondsBetweenExploreMusic + range * modifier;
            if (!LongLargoMain.QueueManager.IsPlaying)
            {
                _instance.m_TimeToPlayNextExploreMusic *= modifier;
            }
        }
    }

    public void OnDestroy()
    {
        LLogger.Debug("SceneMusicManagerImpl destroyed");
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
            return false;
        }

        if (LongLargoMain.QueueManager.IsPlaying)
        {
            return false;
        }
        
        LLogger.Debug("SceneMusicManagerImpl playing explore music");
        
        (var clip, var allowVanilla) = LongLargoMain.QueueManager.GetExplorationClip();
        
        LLogger.Debug($"SceneMusicManagerImpl chosen clip {clip?.audioClip?.name ?? "LongSilence"}");
        
        LongLargoMain.QueueManager.PlaySoft(clip);
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