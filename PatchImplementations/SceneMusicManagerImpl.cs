using System;
using AudioMgr;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppVLB;
using LongLargo.Handlers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo.PatchImplementations;

[RegisterTypeInIl2Cpp]
public class SceneMusicManagerImpl : MonoBehaviour
{
    private SceneMusicManager _instance;
    private Clip _clip;
    
    public SceneMusicManagerImpl(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<SceneMusicManager>();
        if (!Settings.settings.ModEnabled)
        {
            return;
        }

        LLogger.Debug("SceneMusicManagerImpl awakens");
        var delayModifier = Settings.settings.ExplorationDelay;
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
        LongLargoMain.QueueManager.Stop();
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

        if (Settings.settings.ExplorationSuppress)
        {
            return false;
        }

        if (LongLargoMain.QueueManager.IsPlaying)
        {
            return false;
        }
        
        LLogger.Debug("SceneMusicManagerImpl playing explore music");
        
        (_clip, var allowVanilla) = LongLargoMain.QueueManager.GetExplorationClip();
        
        LLogger.Debug($"SceneMusicManagerImpl chosen clip {_clip?.audioClip?.name ?? "LongSilence"}");
        
        var source = gameObject.GetOrAddComponent<AudioSource>();
        source.clip = _clip.audioClip;
        LongLargoMain.QueueManager.PlaySoft(_clip);
        return allowVanilla;
    }

    [HideFromIl2Cpp]
    public void PlayExploreMusicPost()
    {
        if (ShouldSkip()  || Settings.settings.ExplorationSuppress)
        {
            return;
        }
        
    }

    private static bool ShouldSkip()
    {
        if (!Settings.settings.ModEnabled)
        {
            return true;
        }

        var scene = GameManager.m_ActiveScene;
        if (scene.Contains("Menu") || scene.Contains("Boot"))
        {
            return true;
        }

        return false;
    }
}