using HarmonyLib;
using Il2Cpp;
using Il2CppVLB;
using LongLargo.Handlers;

namespace LongLargo.Patches;

internal class SceneMusicManagerPatches
{
    // Hook for music delay
    [HarmonyPatch(typeof(SceneMusicManager), "Awake")]
    internal class SceneMusicManagerAwake
    {
        private static void Postfix(SceneMusicManager __instance)
        {
            __instance.gameObject.GetOrAddComponent<SceneMusicManagerHandler>();
        }
    }

    // Hook for playing exploration music
    [HarmonyPatch(typeof(SceneMusicManager), "PlayExploreMusic")]
    internal class SceneMusicManagerPlayExploreMusicOverride
    {
        private static bool Prefix(SceneMusicManager __instance)
        {
            return __instance.gameObject.GetComponent<SceneMusicManagerHandler>().PlayExploreMusic();
        }
    }
}