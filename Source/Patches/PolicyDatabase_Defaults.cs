using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch(typeof(OutfitDatabase), nameof(OutfitDatabase.SetDefault))]
    internal static class OutfitDatabase_SetDefault
    {
        internal static void Postfix(OutfitDatabase __instance)
        {
            if (ReferenceEquals(__instance, Current.Game?.outfitDatabase))
                AssignManager._defaultOutfit = __instance.DefaultOutfit();
        }
    }

    [HarmonyPatch(typeof(FoodRestrictionDatabase), nameof(FoodRestrictionDatabase.SetDefault))]
    internal static class FoodRestrictionDatabase_SetDefault
    {
        internal static void Postfix(FoodRestrictionDatabase __instance)
        {
            if (ReferenceEquals(__instance, Current.Game?.foodRestrictionDatabase))
                AssignManager._defaultFoodPolicy = __instance.DefaultFoodRestriction();
        }
    }

    [HarmonyPatch(typeof(DrugPolicyDatabase), nameof(DrugPolicyDatabase.SetDefault))]
    internal static class DrugPolicyDatabase_SetDefault
    {
        internal static void Postfix(DrugPolicyDatabase __instance)
        {
            if (ReferenceEquals(__instance, Current.Game?.drugPolicyDatabase))
                AssignManager._defaultDrugPolicy = __instance.DefaultDrugPolicy();
        }
    }

    [HarmonyPatch(typeof(DrugPolicyDatabase), nameof(DrugPolicyDatabase.MakePolicyDefault))]
    internal static class DrugPolicyDatabase_MakePolicyDefault
    {
        internal static void Postfix(DrugPolicyDatabase __instance)
        {
            DrugPolicyDatabase_SetDefault.Postfix(__instance);
        }
    }

    [HarmonyPatch(typeof(ReadingPolicyDatabase), nameof(ReadingPolicyDatabase.SetDefault))]
    internal static class ReadingPolicyDatabase_SetDefault
    {
        internal static void Postfix(ReadingPolicyDatabase __instance)
        {
            if (ReferenceEquals(__instance, Current.Game?.readingPolicyDatabase))
                AssignManager._defaultReadingPolicy = __instance.DefaultReadingPolicy();
        }
    }
}
