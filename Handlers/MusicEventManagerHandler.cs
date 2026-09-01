using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using LongLargo.Managers;
using LongLargo.Models;
using LongLargo.Utils;
using MelonLoader;
using UnityEngine;

namespace LongLargo.Handlers;

[RegisterTypeInIl2Cpp]
public class MusicEventManagerHandler : MonoBehaviour
{
    private MusicEventManager _instance;
    
    private bool isStalked = false;
    private bool isAttacked = false;

    public MusicEventManagerHandler(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<MusicEventManager>();
    }
    
    [HideFromIl2Cpp]
    public void CheckForBeingStalkedPost()
    {
        // TODO: Need to find a proper way to start and finish Timberwolf combat because stop event is unreliable
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
        return !SettingsManager.Settings.ConditionSuppress;
    }

    [HideFromIl2Cpp]
    public bool CheckForSorrow()
    {
        return !SettingsManager.Settings.ConditionSuppress;
    }

    [HideFromIl2Cpp]
    public void PlayLocationSound(bool hasPlayedBefore)
    {
        LLogger.Debug($"[PlayLocationSound] {hasPlayedBefore}");
    }
}