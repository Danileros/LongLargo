using System;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using LongLargo.Handlers;
using LongLargo.Model;
using MelonLoader;
using UnityEngine;

namespace LongLargo.PatchImplementations;

[RegisterTypeInIl2Cpp]
public class MusicEventManagerImpl : MonoBehaviour
{
    private MusicEventManager _instance;
    
    private bool isStalked = false;
    private bool isAttacked = false;

    public MusicEventManagerImpl(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<MusicEventManager>();
    }
    
    [HideFromIl2Cpp]
    public bool CheckForBeingStalkedPre()
    {
        return !LLSettings.settings.StalkedSuppress;
    }
    
    [HideFromIl2Cpp]
    public void CheckForBeingStalkedPost()
    {
        var isStalkedNew = _instance.m_WasBeingStalkedLastFrame;
        if (isStalkedNew != isStalked)
        {
            isStalked = isStalkedNew;
            LLogger.Debug($"[CheckForBeingStalkedPost] New stalked status: {isStalked}");
        }
        
        var isAttackedNew = _instance.m_WasBeingAttackedLastFrame;
        if (isAttackedNew != isAttacked)
        {
            isAttacked = isAttackedNew;
            LLogger.Debug($"[CheckForBeingStalkedPost] New attacked status: {isStalked}");
        }
    }

    [HideFromIl2Cpp]
    public bool CheckForHappySuccess()
    {
        return !LLSettings.settings.ConditionSuppress;
    }

    [HideFromIl2Cpp]
    public bool CheckForSorrow()
    {
        return !LLSettings.settings.StalkedSuppress;
    }

    [HideFromIl2Cpp]
    public void PlayLocationSound(bool hasPlayedBefore)
    {
        LLogger.Debug($"[PlayLocationSound] {hasPlayedBefore}");
    }
}