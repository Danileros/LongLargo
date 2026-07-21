using HarmonyLib;
using Il2Cpp;
using Il2CppVLB;
using LongLargo.PatchImplementations;

namespace LongLargo.Patchers;

internal class MusicEventManagerPatches
{
    [HarmonyPatch(typeof(MusicEventManager), "CheckForBeingStalked")]
    internal class MusicEventManager_CheckForBeingStalked
    {
        internal static bool Prefix(MusicEventManager __instance)
        {
            return __instance.GetOrAddComponent<MusicEventManagerImpl>().CheckForBeingStalkedPre();
        }
        internal static void Postfix(MusicEventManager __instance)
        {
            __instance.GetOrAddComponent<MusicEventManagerImpl>().CheckForBeingStalkedPost();
        }
    }
    
    [HarmonyPatch(typeof(MusicEventManager), "CheckForHappySuccess")]
    internal class MusicEventManager_CheckForHappySuccess
    {
        internal static bool Prefix(MusicEventManager __instance)
        {
            return __instance.GetOrAddComponent<MusicEventManagerImpl>().CheckForHappySuccess();
        }
    }
    
    [HarmonyPatch(typeof(MusicEventManager), "CheckForSorrow")]
    internal class MusicEventManager_CheckForSorrow
    {
        internal static bool Prefix(MusicEventManager __instance)
        {
            return __instance.GetOrAddComponent<MusicEventManagerImpl>().CheckForSorrow();
        }
    }

    [HarmonyPatch(typeof(MusicEventManager), "PlayLocationSound")]
    internal class MusicEventManager_PlayLocationSound
    {
        private static void Postfix(MusicEventManager __instance, ref bool hasPlayedBefore)
        {
            __instance.GetOrAddComponent<MusicEventManagerImpl>().PlayLocationSound(hasPlayedBefore);
        }
    }
}