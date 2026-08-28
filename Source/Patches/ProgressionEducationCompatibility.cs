using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlForked.Patches
{
    // Built-in version of Machado's BetterPawnControl ProgressionEducation Patch:
    // https://steamcommunity.com/sharedfiles/filedetails/?id=3673605975
    internal static class ProgressionEducationCompatibility
    {
        private const string PackageId = "ferny.ProgressionEducation";
        private const int DefaultClassPolicyId = 0;
        private static bool disabled;
        private static bool failureLogged;

        private static readonly Type TimeAssignmentUtilityType = AccessTools.TypeByName("ProgressionEducation.TimeAssignmentUtility");
        private static readonly MethodInfo IsStudyGroupAssignmentMethod = TimeAssignmentUtilityType == null
            ? null
            : AccessTools.Method(TimeAssignmentUtilityType, "IsStudyGroupAssignment");

        internal static bool Available
        {
            get
            {
                return !disabled && LoadedModManager.RunningModsListForReading.Any(
                    mod => string.Equals(mod.PackageId, PackageId, StringComparison.OrdinalIgnoreCase));
            }
        }

        internal static MethodBase SetPawnSchedulesMethod()
        {
            return TimeAssignmentUtilityType == null
                ? null
                : AccessTools.Method(TimeAssignmentUtilityType, "SetPawnSchedules");
        }

        internal static bool SyncBpcClassSchedule(object[] args)
        {
            if (!Available || args == null || args.Length < 2)
            {
                return true;
            }

            object studyGroup = args[0];
            List<Pawn> participants = args[1] as List<Pawn>;
            TimeAssignmentDef assignment = args.Length > 2 ? args[2] as TimeAssignmentDef : null;
            if (studyGroup == null || participants == null)
            {
                return true;
            }

            Map targetMap = ReadMember<Map>(studyGroup, "Map") ?? Find.CurrentMap;
            if (targetMap == null)
            {
                return true;
            }

            int startHour = ReadMember<int>(studyGroup, "startHour");
            int endHour = ReadMember<int>(studyGroup, "endHour");
            int targetMapId = targetMap.uniqueID;

            foreach (Pawn participant in participants)
            {
                if (!PawnCompatibility.SupportsSchedule(participant))
                {
                    continue;
                }

                ScheduleLink link = GetOrCreateScheduleLink(participant, DefaultClassPolicyId, targetMapId);
                if (link == null)
                {
                    continue;
                }

                ApplyStudyGroupHours(link, participant, startHour, endHour, assignment);
            }

            Policy activePolicy = ScheduleManager.GetActivePolicy(targetMapId);
            return activePolicy == null || activePolicy.id == DefaultClassPolicyId;
        }

        internal static void RemoveClassesFromPawnTimetables(List<Pawn> pawns)
        {
            if (!Available || IsStudyGroupAssignmentMethod == null || pawns == null)
            {
                return;
            }

            foreach (Pawn pawn in pawns)
            {
                if (pawn?.timetable == null)
                {
                    continue;
                }

                for (int hour = 0; hour < 24; hour++)
                {
                    TimeAssignmentDef current = pawn.timetable.GetAssignment(hour);
                    if (IsStudyGroupAssignment(current))
                    {
                        pawn.timetable.SetAssignment(hour, ScheduleLink.DefaultAssignment(hour));
                    }
                }
            }
        }

        private static ScheduleLink GetOrCreateScheduleLink(Pawn pawn, int policyId, int mapId)
        {
            ScheduleLink link = ScheduleManager.links.FirstOrDefault(
                l => l != null
                    && l.zone == policyId
                    && l.mapId == mapId
                    && l.colonist != null
                    && PawnCompatibility.SamePawn(l.colonist, pawn));

            if (link != null)
            {
                link.RepairSchedule();
                return link;
            }

            link = new ScheduleLink(
                policyId,
                pawn,
                pawn.playerSettings?.AreaRestrictionInPawnCurrentMap,
                pawn.timetable?.times,
                mapId);
            link.RepairSchedule();
            ScheduleManager.links.Add(link);
            return link;
        }

        private static void ApplyStudyGroupHours(ScheduleLink link, Pawn pawn, int startHour, int endHour, TimeAssignmentDef assignment)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                bool scheduled = startHour <= endHour
                    ? hour >= startHour && hour <= endHour
                    : hour >= startHour || hour <= endHour;

                if (scheduled)
                {
                    TimeAssignmentDef assignmentToSet = assignment ?? ScheduleLink.DefaultAssignment(hour);
                    if (!link.TrySetAssignment(hour, assignmentToSet))
                    {
                        Log.Warning("[BPC] ProgressionEducation schedule sync skipped invalid hour " + hour + " for " + pawn.ToStringSafe());
                        return;
                    }
                }
            }
        }

        private static bool IsStudyGroupAssignment(TimeAssignmentDef assignment)
        {
            try
            {
                object result = IsStudyGroupAssignmentMethod.Invoke(null, new object[] { assignment });
                return result is bool value && value;
            }
            catch (Exception ex)
            {
                Disable("assignment check failed", ex);
                return false;
            }
        }

        private static T ReadMember<T>(object instance, string name)
        {
            Type type = instance.GetType();
            PropertyInfo property = AccessTools.Property(type, name);
            if (property != null && property.GetValue(instance, null) is T propertyValue)
            {
                return propertyValue;
            }

            FieldInfo field = AccessTools.Field(type, name);
            if (field != null && field.GetValue(instance) is T fieldValue)
            {
                return fieldValue;
            }

            return default(T);
        }

        private static void Disable(string reason, Exception exception)
        {
            disabled = true;
            if (failureLogged)
            {
                return;
            }

            failureLogged = true;
            Log.Warning("[BPC] Progression: Education integration disabled: " + reason + ". " + exception.GetBaseException().Message);
        }


    }

    [HarmonyPatch]
    internal static class ProgressionEducation_SetPawnSchedules_Patch
    {
        public static bool Prepare()
        {
            return ProgressionEducationCompatibility.Available && ProgressionEducationCompatibility.SetPawnSchedulesMethod() != null;
        }

        public static MethodBase TargetMethod()
        {
            return ProgressionEducationCompatibility.SetPawnSchedulesMethod();
        }

        public static bool Prefix(object[] __args)
        {
            return ProgressionEducationCompatibility.SyncBpcClassSchedule(__args);
        }
    }

    [HarmonyPatch(typeof(ScheduleManager), nameof(ScheduleManager.LoadState), typeof(List<ScheduleLink>), typeof(List<Pawn>), typeof(Policy))]
    internal static class ProgressionEducation_ScheduleManager_LoadState_Patch
    {
        public static void Postfix(List<Pawn> pawns, Policy policy)
        {
            if (policy != null && policy.id != 0)
            {
                ProgressionEducationCompatibility.RemoveClassesFromPawnTimetables(pawns);
            }
        }
    }
}
