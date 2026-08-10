using System.Text;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using LongLargo.Utils;
using MelonLoader;
using UnityEngine;

namespace LongLargo.Handlers;

[RegisterTypeInIl2Cpp]
public class PackManagerHandler : MonoBehaviour
{
    private PackManager _instance;
    private float _distance;

    public PackManagerHandler(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<PackManager>();
        Main.DebugManager.RegisterDebugCommand("ll_debug_packs", RefreshDebug);
    }

    public void OnDestroy()
    {
        Main.DebugManager.UnregisterDebugCommand("ll_debug_packs");
    }

    [HideFromIl2Cpp]
    public void Refresh(PackManager packManager)
    {
        _instance = packManager;

        _distance = GetDistance();
        Main.PackProximityManager.UpdateMusic(_distance);
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

    private string RefreshDebug()
    {
        var player = GameManager.m_vpFPSPlayer.transform;
        var sb = new StringBuilder();
        sb.AppendLine($"Closest: {_distance:F2}");
        sb.AppendLine($"Fadeout timer: {Main.PackProximityManager.FadeoutTimer}");
        
        foreach (var packinfo in _instance.m_PackAnimalGroupByLeader)
        {
            var leader = packinfo.Key;
            var group = packinfo.Value;
            var lPos = leader.transform.position;
            var lDistance = Vector3.Distance(player.position, lPos);
            // sb.AppendLine($"Leader: {lDistance:F2} | {leader.m_PackMode}");
            sb.AppendLine($"Pack:   {group.m_TargetAwarenessTime:F2} | {group.m_PackMoraleModifier}");
            foreach (var animal in group.m_Members)
            {
                var aPos = animal.transform.position;
                var aDistance = Vector3.Distance(player.position, aPos);
                sb.AppendLine($"Animal: {aDistance:F2} | {animal.m_StayWithinRadius}");
            }
        }
        
        return sb.ToString();
    }
}