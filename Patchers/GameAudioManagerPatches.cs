using System;
using HarmonyLib;
using Il2Cpp;
using LongLargo.Handlers;
using LongLargo.PatchImplementations;
using UnityEngine;

namespace LongLargo.Patchers;

internal class GameAudioManagerPatches
{
    [HarmonyPatch(typeof(GameAudioManager), "PlayMusic", new Type[] { typeof(string), typeof(GameObject) })]
    public class GameAudio_PlayMusic
    {
        public static bool Prefix(ref string soundID, ref GameObject go)
        {
            // LLogger.Log($"[PlayMusic] str soundID: {soundID}");
            return GameAudioManagerImpl.PlayMusic(soundID, ref go);
        }
    }

    [HarmonyPatch(typeof(GameAudioManager), "PlayMusic", new Type[] { typeof(uint), typeof(GameObject) })]
    public class GameAudio_PlayMusic2
    {
        public static bool Prefix(ref uint soundID, ref GameObject go)
        {
            var eventName = EventIdProvider.GetEventName(soundID);
            // LLogger.Log($"[PlayMusic] int soundID: {eventName}");
            return GameAudioManagerImpl.PlayMusic(eventName, ref go);
        }
    }
     
    // Weather music for some reason plays here instead of PlayMusic
    [HarmonyPatch(typeof(GameAudioManager), "PlaySound", new Type[] { typeof(Il2CppAK.Wwise.Event), typeof(GameObject) })]
    public class GameAudioManager_PlaySound
    {
        public static bool Prefix(ref Il2CppAK.Wwise.Event soundEvent, ref GameObject go)
        {
            // LLogger.Debug($"[PlaySound] ev soundID: {soundEvent.Name}");
            return GameAudioManagerImpl.PlayMusic(soundEvent.Name, ref go);
        }
    }
        
    [HarmonyPatch(typeof(GameAudioManager), "PlaySound", new Type[] { typeof(string), typeof(GameObject) })]
    public class GameAudioManager_PlaySound2
    {
        public static bool Prefix(ref string soundID, ref GameObject go)
        {
            // LLogger.Debug($"[PlaySound] str soundID: {soundID}");
            return GameAudioManagerImpl.PlayMusic(soundID, ref go);
        }
    }
}