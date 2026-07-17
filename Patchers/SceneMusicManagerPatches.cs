using HarmonyLib;
using Il2Cpp;
using Il2CppVLB;
using LongLargo.PatchImplementations;

namespace LongLargo.Patchers;

internal class SceneMusicManagerPatches
{
    // Hook for music delay
    [HarmonyPatch(typeof(SceneMusicManager), "Awake")]
    internal class SceneMusicManagerAwake
    {
        private static void Postfix(SceneMusicManager __instance)
        {
            __instance.gameObject.GetOrAddComponent<SceneMusicManagerImpl>();
        }
    }

    // Hook for playing exploration music
    [HarmonyPatch(typeof(SceneMusicManager), "PlayExploreMusic")]
    internal class SceneMusicManagerPlayExploreMusicOverride
    {
        private static bool Prefix(SceneMusicManager __instance)
        {
            return __instance.gameObject.GetComponent<SceneMusicManagerImpl>().PlayExploreMusic();
        }
    }
}