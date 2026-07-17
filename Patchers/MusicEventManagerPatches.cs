using HarmonyLib;
using Il2Cpp;
using LongLargo.Handlers;
using LongLargo.Model;

namespace LongLargo.Patchers;

internal class MusicEventManagerPatches
{
    [HarmonyPatch(typeof(MusicEventManager), "CheckForBeingStalked")]
    internal class MusicEventManager_CheckForBeingStalked
    {
        internal static bool Prefix()
        {
            return !LLSettings.settings.StalkedSuppress;
        }
    }
    
    [HarmonyPatch(typeof(MusicEventManager), "CheckForHappySuccess")]
    internal class MusicEventManager_CheckForHappySuccess
    {
        internal static bool Prefix()
        {
            return !LLSettings.settings.SuccessSuppress;
        }
    }
    
    [HarmonyPatch(typeof(MusicEventManager), "CheckForSorrow")]
    internal class MusicEventManager_CheckForSorrow
    {
        internal static bool Prefix()
        {
            return !LLSettings.settings.StalkedSuppress;
        }
    }

    [HarmonyPatch(typeof(MusicEventManager), "PlayLocationSound")]
    internal class MusicEventManager_PlayLocationSound
    {
        private static void Postfix(MusicEventManager __instance, ref bool hasPlayedBefore)
        {
            LLogger.Debug($"[PlayLocationSound] {hasPlayedBefore}");
        }
    }
}