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
    
    
    private Dictionary<PackGroup, float> morales = new Dictionary<PackGroup, float>(4);

    public PackManagerHandler(IntPtr intPtr)  : base(intPtr) { }

    public void Awake()
    {
        _instance = gameObject.GetComponent<PackManager>();
        Main.DebugManager.RegisterDebugCommand("ll_debug_packs", DebugData);
    }

    public void OnDestroy()
    {
        Main.DebugManager.UnregisterDebugCommand("ll_debug_packs");
    }

    [HideFromIl2Cpp]
    public void Refresh(PackManager packManager)
    {
        _instance = packManager;

        var distance = GetDistance();
        var moraleChanged = IsMoraleChanged();
        Main.PackCombatManager.UpdateMusic(distance, moraleChanged);
    }

    private float GetDistance()
    {
        if (Main.PackCombatManager.IsInCombat
            && !GameManager.m_IsPaused
            && !GameManager.s_IsGameplaySuspended
            && !GameManager.s_IsAISuspended)
        {
            try
            {
                var distance = 99999f;
                var playerPosition = GameManager.GetPlayerTransform().position;
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

    private bool IsMoraleChanged()
    {
        var moraleChanged = false;
        var groupsInCombat = new List<PackGroup>(_instance.m_PackAnimalGroupByLeader.Count);
        foreach (var packinfo in _instance.m_PackAnimalGroupByLeader)
        {
            var group = packinfo.Value;
            var moraleModifier = group.m_PackMoraleModifier;
            if (!morales.TryGetValue(group, out var morale) || !Mathf.Approximately(moraleModifier, morale))
            {
                moraleChanged = true;
            }
            
            morales[group] = moraleModifier;
            groupsInCombat.Add(group);
        }

        var groupsToRemove = morales
            .Keys
            .Where(g => !groupsInCombat.Contains(g))
            .ToArray();
        if (groupsToRemove.Length > 0)
        {
            moraleChanged = true;
            foreach (var group in groupsToRemove)
            {
                morales.Remove(group);
            }
        }
        
        return moraleChanged;
    }

    private string DebugData()
    {
        if (GameManager.m_vpFPSPlayer == null)
        {
            return "";
        }
        
        var player = GameManager.m_vpFPSPlayer.transform;
        var sb = new StringBuilder();
        foreach (var packinfo in _instance.m_PackAnimalGroupByLeader)
        {
            var leader = packinfo.Key;
            var group = packinfo.Value;
            var lPos = leader.transform.position;
            sb.AppendLine($"Pack:   {group.m_TargetAwarenessTime:F2} | {group.m_PackMoraleModifier:F2}");
            foreach (var animal in group.m_Members)
            {
                var aPos = animal.transform.position;
                var aDistance = Vector3.Distance(player.position, aPos);
                sb.AppendLine($"Animal: {aDistance:F2} | {animal.m_StayWithinRadius} | {animal.m_BaseAi.m_HoldGroundDistanceFromFire:F2} | {animal.m_BaseAi.m_HoldGroundOuterDistanceFromFire:F2}");
            }
        }
        
        return sb.ToString();
    }
}