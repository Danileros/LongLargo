// using HarmonyLib;
// using Il2Cpp;
// using LongLargo.Handlers;
// using LongLargo.Helpers;
// using UnityEngine;
//
// namespace LongLargo.Patches;
//
// [HarmonyPatch(typeof(AkSoundEngine))]
// [HarmonyPatch(nameof(AkSoundEngine.PostEvent))]
// [HarmonyPatch(new[] { typeof(string), typeof(GameObject) })]
// public class WwisePostEventPatch1
// {
//     static bool Prefix(string in_pszEventName, GameObject in_gameObjectID)
//     {
//         LLogger.Debug($"[WWise PostEvent1]: {in_pszEventName}");
//         return AkSoundEngineHandler.PostEvent(in_pszEventName); 
//     }
// }
//
// [HarmonyPatch(typeof(AkSoundEngine))]
// [HarmonyPatch(nameof(AkSoundEngine.PostEvent))]
// [HarmonyPatch(new[] { typeof(string), typeof(GameObject), typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(Il2CppSystem.Object) })]
// public class WwisePostEventPatch2
// {
//     static bool Prefix(
//         string in_pszEventName,
//         GameObject in_gameObjectID,
//         uint in_uFlags,
//         AkCallbackManager.EventCallback in_pfnCallback,
//         Il2CppSystem.Object in_pCookie)
//     {
//         LLogger.Debug($"[WwisePostEvent2]: {in_pszEventName}");
//         return AkSoundEngineHandler.PostEvent(in_pszEventName); 
//     }
// }
//
// [HarmonyPatch(typeof(AkSoundEngine))]
// [HarmonyPatch(nameof(AkSoundEngine.PostEvent))]
// [HarmonyPatch(new[] { typeof(uint), typeof(GameObject), typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(Il2CppSystem.Object) })]
// public class WwisePostEventPatch3
// {
//     static bool Prefix(
//         uint in_eventID,
//         GameObject in_gameObjectID,
//         uint in_uFlags,
//         AkCallbackManager.EventCallback in_pfnCallback,
//         Il2CppSystem.Object in_pCookie)
//     {
//         var name = EventIdProvider.GetEventName(in_eventID);
//         LLogger.Debug($"[WwisePostEvent3]: {in_eventID}: {name}");
//         return AkSoundEngineHandler.PostEvent(name); 
//     }
// }
//
//
// [HarmonyPatch(typeof(AkSoundEngine))]
// [HarmonyPatch(nameof(AkSoundEngine.PostEvent))]
// [HarmonyPatch(new[] { typeof(uint), typeof(GameObject) })] // Targeting the String/GameObject overload
// public class WwisePostEventPatch4
// {
//     static bool Prefix(uint in_eventID, GameObject in_gameObjectID)
//     {
//         var name = EventIdProvider.GetEventName(in_eventID);
//         LLogger.Debug($"[WwisePostEvent4]: {in_eventID}: {name}");
//         return AkSoundEngineHandler.PostEvent(name); 
//     }
// }