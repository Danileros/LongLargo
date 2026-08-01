using HarmonyLib;
using Il2Cpp;
using Il2CppVLB;
using LongLargo.Handlers;

namespace LongLargo.Patches;

internal class PackManagerPatches
{
    [HarmonyPatch(typeof(PackManager), "Update")]
    internal class PackManager_Update
    {
        private static PackManagerHandler _handler = null;
        
        private static void Postfix(PackManager __instance)
        {
            if (_handler == null)
            {
                _handler = __instance.GetOrAddComponent<PackManagerHandler>();
            }

            _handler.RefreshDebug(__instance);
        }
    }
}