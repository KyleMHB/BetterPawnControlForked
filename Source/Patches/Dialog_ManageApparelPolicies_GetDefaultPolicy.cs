using HarmonyLib;
using RimWorld;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch(typeof(Dialog_ManageApparelPolicies), "GetDefaultPolicy")]
    static class Dialog_ManageApparelPolicies_GetDefaultPolicy
    {
        static void Postfix(ref ApparelPolicy __result)
        {
            __result = AssignManager.DefaultOutfit;
        }
        
    }
}


