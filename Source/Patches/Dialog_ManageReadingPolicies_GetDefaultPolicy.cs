using HarmonyLib;
using RimWorld;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch(typeof(Dialog_ManageReadingPolicies), "GetDefaultPolicy")]
    static class Dialog_ManageReadingPolicies_GetDefaultPolicy
    {
        static void Postfix(ref ReadingPolicy __result)
        {
            __result = AssignManager.DefaultReadingPolicy;
        }

    }
}


