using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BetterPawnControlForked.DevTests;

// One BPC tab manager (Work, Schedule, ...) reached by reflection, like Bpc.cs does for Assign.
internal sealed class BpcTab
{
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    private static readonly FieldInfo PolicyId = AccessTools.Field(typeof(Policy), "id");

    public readonly string Name;
    private readonly PropertyInfo policiesProp, linksProp;
    private readonly MethodInfo getActive, saveState, loadStateFull;

    public BpcTab(string managerName, Type linkType)
    {
        Name = managerName;
        var manager = AccessTools.TypeByName("BetterPawnControlForked." + managerName)
            ?? throw new InvalidOperationException(managerName + " not found");
        var baseType = AccessTools.TypeByName("BetterPawnControlForked.Manager`1").MakeGenericType(linkType);
        policiesProp = baseType.GetProperty("policies", Static) ?? throw new InvalidOperationException(managerName + ".policies not found");
        linksProp = baseType.GetProperty("links", Static) ?? throw new InvalidOperationException(managerName + ".links not found");
        getActive = baseType.GetMethod("GetActivePolicy", Static, null, Type.EmptyTypes, null) ?? throw new InvalidOperationException(managerName + ".GetActivePolicy not found");
        saveState = manager.GetMethod("SaveCurrentState", Static, null, new[] { typeof(List<Pawn>) }, null)
            ?? throw new InvalidOperationException(managerName + ".SaveCurrentState not found");
        loadStateFull = manager.GetMethod("LoadState", Static, null, new[] { typeof(List<>).MakeGenericType(linkType), typeof(List<Pawn>), typeof(Policy) }, null)
            ?? throw new InvalidOperationException(managerName + ".LoadState(links, pawns, policy) not found");
    }

    public List<Policy> Policies => (List<Policy>)policiesProp.GetValue(null);
    public Policy Active => (Policy)getActive.Invoke(null, null);
    public static int Id(Policy policy) => (int)PolicyId.GetValue(policy);
    public Policy Find(int id) => Policies.FirstOrDefault(p => Id(p) == id);

    public Policy Add(string label)
    {
        var policy = new Policy(Policies.Max(Id) + 1, label);
        Policies.Add(policy);
        return policy;
    }

    // The tab's active-policy button saves the current state, then the chosen menu option loads the policy.
    public void Select(List<Pawn> pawns, Policy policy)
    {
        saveState.Invoke(null, new object[] { pawns });
        loadStateFull.Invoke(null, new object[] { linksProp.GetValue(null), pawns, policy });
    }
}

// The emergency feature: AlertManager's level settings and the play-settings toggle the button and keybind call.
internal static class BpcEmergency
{
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    private static readonly Type Alert = AccessTools.TypeByName("BetterPawnControlForked.AlertManager")
        ?? throw new InvalidOperationException("AlertManager not found");
    private static readonly Type ResourceType = AccessTools.TypeByName("BetterPawnControlForked.Resources+Type")
        ?? throw new InvalidOperationException("Resources.Type not found");
    private static readonly Type Toggle = AccessTools.TypeByName("BetterPawnControlForked.Patches.PlaySettings_DoPlaySettingsGlobalControls")
        ?? throw new InvalidOperationException("PlaySettings_DoPlaySettingsGlobalControls not found");
    private static readonly PropertyInfo OnAlertProp = Alert.GetProperty("OnAlert", Static) ?? throw new InvalidOperationException("AlertManager.OnAlert not found");
    private static readonly MethodInfo GetAlert = Alert.GetMethod("GetAlertPolicy", Static) ?? throw new InvalidOperationException("AlertManager.GetAlertPolicy not found");
    private static readonly MethodInfo SaveLevel = Alert.GetMethod("SaveState", Static, null, new[] { typeof(int), ResourceType, typeof(Policy) }, null)
        ?? throw new InvalidOperationException("AlertManager.SaveState(level, type, policy) not found");
    private static readonly MethodInfo On = AccessTools.Method(Toggle, "EmergencyToogleON") ?? throw new InvalidOperationException("EmergencyToogleON not found");
    private static readonly MethodInfo Off = AccessTools.Method(Toggle, "EmergencyToogleOFF") ?? throw new InvalidOperationException("EmergencyToogleOFF not found");

    private static object Type(string name) => Enum.Parse(ResourceType, name);

    public static bool OnAlert => (bool)OnAlertProp.GetValue(null);
    public static Policy PolicyFor(int level, string type) => (Policy)GetAlert.Invoke(null, new[] { level, Type(type) });

    // The Manage Policies dialog's emergency menu option for one tab.
    public static void SetPolicy(int level, string type, Policy policy) => SaveLevel.Invoke(null, new[] { level, Type(type), policy });

    public static void TurnOn() => On.Invoke(null, new object[] { null });
    public static void TurnOff() => Off.Invoke(null, new object[] { null });
}
