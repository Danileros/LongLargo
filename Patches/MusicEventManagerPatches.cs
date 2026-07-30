using HarmonyLib;
using Il2Cpp;
using Il2CppVLB;
using LongLargo.Handlers;

namespace LongLargo.Patches;

internal class MusicEventManagerPatches
{
    [HarmonyPatch(typeof(MusicEventManager), "CheckForBeingStalked")]
    internal class MusicEventManager_CheckForBeingStalked
    {
        internal static void Postfix(MusicEventManager __instance)
        {
            __instance.GetOrAddComponent<MusicEventManagerHandler>().CheckForBeingStalkedPost();
        }
    }
    
    [HarmonyPatch(typeof(MusicEventManager), "CheckForHappySuccess")]
    internal class MusicEventManager_CheckForHappySuccess
    {
        internal static bool Prefix(MusicEventManager __instance)
        {
            return __instance.GetOrAddComponent<MusicEventManagerHandler>().CheckForHappySuccess();
        }
    }
    
    [HarmonyPatch(typeof(MusicEventManager), "CheckForSorrow")]
    internal class MusicEventManager_CheckForSorrow
    {
        internal static bool Prefix(MusicEventManager __instance)
        {
            return __instance.GetOrAddComponent<MusicEventManagerHandler>().CheckForSorrow();
        }
    }

    [HarmonyPatch(typeof(MusicEventManager), "PlayLocationSound")]
    internal class MusicEventManager_PlayLocationSound
    {
        private static void Postfix(MusicEventManager __instance, ref bool hasPlayedBefore)
        {
            __instance.GetOrAddComponent<MusicEventManagerHandler>().PlayLocationSound(hasPlayedBefore);
        }
    }
}