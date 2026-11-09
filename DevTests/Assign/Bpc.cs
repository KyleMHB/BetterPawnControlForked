using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BetterPawnControlForked.DevTests;

// The production managers are internal and the release DLL has no InternalsVisibleTo, so the cases reach them by reflection.
// Each member is resolved once; a missing member throws at startup of the case rather than silently skipping.
internal static class Bpc
{
    private static readonly Type AssignManager = AccessTools.TypeByName("BetterPawnControlForked.AssignManager")
        ?? throw new InvalidOperationException("BetterPawnControlForked.AssignManager not found");
    private static readonly Type AssignBase = AccessTools.TypeByName("BetterPawnControlForked.Manager`1").MakeGenericType(typeof(AssignLink));

    private static readonly PropertyInfo PoliciesProp = Prop(AssignBase, "policies");
    private static readonly PropertyInfo LinksProp = Prop(AssignBase, "links");
    private static readonly MethodInfo GetActive = Method(AssignBase, "GetActivePolicy", Type.EmptyTypes);
    private static readonly MethodInfo SaveState = Method(AssignManager, "SaveCurrentState", new[] { typeof(List<Pawn>) });
    private static readonly MethodInfo LoadStateFull = Method(AssignManager, "LoadState", new[] { typeof(List<AssignLink>), typeof(List<Pawn>), typeof(Policy) });
    private static readonly MethodInfo Delete = Method(AssignManager, "DeletePolicy", new[] { typeof(Policy) });
    private static readonly FieldInfo PolicyId = AccessTools.Field(typeof(Policy), "id");
    private static readonly FieldInfo LinkZone = AccessTools.Field(typeof(Link), "zone");
    private static readonly FieldInfo LinkOutfit = AccessTools.Field(typeof(AssignLink), "outfit");
    private static readonly FieldInfo LinkColonist = AccessTools.Field(typeof(AssignLink), "colonist");

    public static List<Policy> Policies => (List<Policy>)PoliciesProp.GetValue(null);
    public static List<AssignLink> Links => (List<AssignLink>)LinksProp.GetValue(null);
    public static Policy Active => (Policy)GetActive.Invoke(null, null);
    public static int Id(Policy policy) => (int)PolicyId.GetValue(policy);
    public static int Zone(AssignLink link) => (int)LinkZone.GetValue(link);
    public static Pawn Colonist(AssignLink link) => (Pawn)LinkColonist.GetValue(link);
    public static string OutfitLabel(AssignLink link) => ((RimWorld.ApparelPolicy)LinkOutfit.GetValue(link))?.label;

    // The Assign tab's active-policy button saves the current state, then the chosen menu option loads the policy.
    public static void SelectPolicy(List<Pawn> pawns, Policy policy)
    {
        SaveState.Invoke(null, new object[] { pawns });
        LoadStateFull.Invoke(null, new object[] { Links, pawns, policy });
    }

    public static void DeletePolicy(Policy policy) => Delete.Invoke(null, new object[] { policy });

    private static PropertyInfo Prop(Type type, string name) =>
        type.GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
        ?? throw new InvalidOperationException($"{type.Name}.{name} not found");

    private static MethodInfo Method(Type type, string name, Type[] args) =>
        type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, args, null)
        ?? throw new InvalidOperationException($"{type.Name}.{name} not found");
}
