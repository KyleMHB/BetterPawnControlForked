using HarmonyLib;
using RimWorld;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch(typeof(Dialog_ManageDrugPolicies), "GetDefaultPolicy")]
    static class Dialog_ManageDrugPolicies_GetDefaultPolicy
    {
        static void Postfix(ref DrugPolicy __result)
        {
            __result = AssignManager.DefaultDrugPolicy;
        }

    }
}


