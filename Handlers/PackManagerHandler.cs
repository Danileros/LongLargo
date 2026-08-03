using System.Text;
using Il2Cpp;
using Il2CppAudio.SimpleAudio;
using Il2CppInterop.Runtime.Attributes;
using LongLargo.Utils;
using MelonLoader;
using UnityEngine;

namespace LongLargo.Handlers;

[RegisterTypeInIl2Cpp]
public class PackManagerHandler : MonoBehaviour
{
    private PackManager _instance;
    private bool isDebugMode = false;
    
    public PackManagerHandler(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<PackManager>();
        // uConsole.RegisterCommand("ll_debug_packs", new Action(ToggleDebugPacks));
    }

    public void OnDestroy()
    {
        isDebugMode = false;
        /// uConsole.UnRegisterCommand("ll_debug_packs");
    }

    private void ToggleDebugPacks()
    {
        isDebugMode = !isDebugMode;
    }

    [HideFromIl2Cpp]
    public void Refresh(PackManager packManager)
    {
        _instance = packManager;

        var distance = GetDistance();
        Main.PackProximityManager.UpdateMusic(distance);
        // RefreshDebug(distance);
    }

    private float GetDistance()
    {
        if (Main.PackProximityManager.IsInCombat
            && !GameManager.m_IsPaused
            && !GameManager.s_IsGameplaySuspended
            && !GameManager.s_IsAISuspended)
        {
            try
            {
                var distance = 99999f;
                var playerPosition = GameManager.m_vpFPSPlayer.transform.position;
                foreach (var packinfo in _instance.m_PackAnimalGroupByLeader)
                {
                    var group = packinfo.Value;
                    foreach (var animal in group.m_Members)
                    {
                        var animalPosition = animal.transform.position;
                        var newDistance = Vector3.Distance(playerPosition, animalPosition);
                        if (newDistance < distance)
                        {
                            distance = newDistance;
                        }
                    }
                }

                return distance;
            }
            catch (Exception e)
            {
                LLogger.Error("Can't calculate timberwolf distance");
            }
        }
        
        return 99999f;
    }

    private void RefreshDebug(float distance)
    {
        if (HUDManager.m_ShowDebugInfo || HUDManager.HudDisplayMode == HudDisplayMode.DebugInfo ||
            vp_FPSCamera.m_DebugCamera || Weather.m_DebugWeather || DialogueModeRigFP.s_DebugDialogue ||
            BearHuntRedux.s_DebugBearHuntRedux || Panel_HUD.s_DebugStoryHUD ||
            PlayAudioSimpleManager.s_Debug)
        {
            isDebugMode = false;
            return;
        }

        var debugLines = InterfaceManager.GetPanel<Panel_HUD>().m_Label_DebugLines;
        debugLines.gameObject.SetActive(isDebugMode);
        if (isDebugMode)
        {
            var player = GameManager.m_vpFPSPlayer.transform;
            var sb = new StringBuilder();
            sb.AppendLine($"Closest: {distance:F2}");
            sb.AppendLine($"Fadeout timer: {Main.PackProximityManager.FadeoutTimer}");
            
            foreach (var packinfo in _instance.m_PackAnimalGroupByLeader)
            {
                var leader = packinfo.Key;
                var group = packinfo.Value;
                var lPos = leader.transform.position;
                var lDistance = Vector3.Distance(player.position, lPos);
                sb.AppendLine($"Leader: {lDistance:F2} | {leader.m_PackMode}");
                sb.AppendLine($"Pack:   {group.m_TargetAwarenessTime:F2} | {group.m_PackMoraleModifier}");
                foreach (var animal in group.m_Members)
                {
                    var aPos = animal.transform.position;
                    var aDistance = Vector3.Distance(player.position, aPos);
                    sb.AppendLine($"Animal: {aDistance:F2} | {animal.m_StayWithinRadius}");
                }
            }
            
            debugLines.text = sb.ToString();
        }
    }
}