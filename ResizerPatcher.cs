using UnityEngine;
using HarmonyLib;

namespace Resizer
{
    public static class ResizerPatcher
    {
        private const string HarmonyId = "Infixo.Resizer";
        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) { Debug.Log($"{HarmonyId} PatchAll: already patched!"); return; }
            //Harmony.DEBUG = true;
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll();
            if (Harmony.HasAnyPatches(HarmonyId))
            {
                Debug.Log($"{HarmonyId} methods patched OK");
                patched = true;
                var myOriginalMethods = harmony.GetPatchedMethods();
                foreach (var method in myOriginalMethods)
                    Debug.Log($"{HarmonyId} ...method {method.Name}");
            }
            else
                Debug.Log($"{HarmonyId} ERROR: methods not patched");
            //Harmony.DEBUG = false;
        }

        public static void UnpatchAll()
        {
            if (!patched) { Debug.Log($"{HarmonyId} UnpatchAll: not patched!"); return; }
            //Harmony.DEBUG = true;
            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            patched = false;
            //Harmony.DEBUG = false;
        }
    }

    [HarmonyPatch(typeof(BuildingInfo))]
    public static class BuildingInfo_Patches
    {
        [HarmonyPostfix, HarmonyPatch("InitializePrefab")]
        public static void BuildingInfo_InitializePrefab_Postfix(BuildingInfo __instance)
        {
            //Debug.Log($"BuildingInfo_InitializePrefab_Postfix: prefab {__instance.name} mesh {__instance.m_mesh?.name} props {__instance.m_props?.Length}");
            __instance.ProcessBuildingPrefab();
        }
    }

} // namespace