using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch]
    internal static class BackCompatibility_TypeAliases
    {
        private static readonly Dictionary<string, Type> Aliases = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            { "BetterPawnControl.DataStorage", typeof(DataStorage) },
            { "BetterPawnControl.Policy", typeof(Policy) },
            { "BetterPawnControl.Link", typeof(Link) },
            { "BetterPawnControl.AssignLink", typeof(AssignLink) },
            { "BetterPawnControl.ScheduleLink", typeof(ScheduleLink) },
            { "BetterPawnControl.WorkLink", typeof(WorkLink) },
            { "BetterPawnControl.AnimalLink", typeof(AnimalLink) },
            { "BetterPawnControl.MechLink", typeof(MechLink) },
            { "BetterPawnControl.WeaponsLink", typeof(WeaponsLink) },
            { "BetterPawnControl.RobotLink", typeof(RobotLink) },
            { "BetterPawnControl.MapActivePolicy", typeof(MapActivePolicy) },
            { "BetterPawnControl.AlertLevel", typeof(AlertLevel) }
        };

        internal static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var method in AccessTools.GetDeclaredMethods(typeof(BackCompatibility)))
            {
                if (method.Name == "GetBackCompatibleType")
                {
                    yield return method;
                }
            }
        }

        internal static bool Prefix(ref Type __result, object[] __args)
        {
            var serializedParts = new List<string>();
            foreach (var argument in __args)
            {
                if (argument is string serializedPart && !string.IsNullOrEmpty(serializedPart))
                {
                    serializedParts.Add(serializedPart);
                    if (Aliases.TryGetValue(serializedPart, out var directReplacement))
                    {
                        __result = directReplacement;
                        return false;
                    }
                }
            }

            foreach (var typeName in serializedParts)
            {
                foreach (var namespaceName in serializedParts)
                {
                    if (Aliases.TryGetValue(namespaceName + "." + typeName, out var combinedReplacement))
                    {
                        __result = combinedReplacement;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
