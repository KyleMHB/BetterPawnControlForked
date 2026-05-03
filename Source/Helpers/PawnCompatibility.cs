using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterPawnControlForked
{
    internal static class PawnCompatibility
    {
        internal static List<Pawn> HumanlikePolicyPawns(Map map)
        {
            if (map?.mapPawns == null)
            {
                return new List<Pawn>();
            }

            return map.mapPawns.PawnsInFaction(Faction.OfPlayer)
                .Concat(map.mapPawns.PrisonersOfColonySpawned)
                .Where(IsHumanlikePolicyPawn)
                .Distinct()
                .ToList();
        }

        internal static bool IsHumanlikePolicyPawn(Pawn pawn)
        {
            if (pawn == null || pawn.Destroyed || pawn.Dead)
            {
                return false;
            }

            if (pawn.RaceProps?.Humanlike != true)
            {
                return false;
            }

            return pawn.IsColonist
                || pawn.IsPlayerControlled
                || pawn.IsPrisonerOfColony
                || pawn.IsPrisoner
                || pawn.IsSlave
                || pawn.Faction == Faction.OfPlayer;
        }

        internal static bool SupportsAssign(Pawn pawn)
        {
            return IsHumanlikePolicyPawn(pawn)
                && (SupportsApparel(pawn)
                    || SupportsFood(pawn)
                    || SupportsDrugs(pawn)
                    || SupportsReading(pawn)
                    || SupportsPlayerSettings(pawn)
                    || SupportsInventoryStock(pawn));
        }

        internal static bool SupportsSchedule(Pawn pawn)
        {
            return IsHumanlikePolicyPawn(pawn)
                && (pawn.timetable != null || pawn.playerSettings != null);
        }

        internal static bool SupportsWork(Pawn pawn)
        {
            return IsHumanlikePolicyPawn(pawn) && pawn.workSettings != null;
        }

        internal static bool SupportsWeapons(Pawn pawn)
        {
            return IsHumanlikePolicyPawn(pawn);
        }

        internal static bool SupportsApparel(Pawn pawn)
        {
            return pawn?.outfits != null;
        }

        internal static bool SupportsFood(Pawn pawn)
        {
            return pawn?.foodRestriction != null;
        }

        internal static bool SupportsDrugs(Pawn pawn)
        {
            return pawn?.drugs != null;
        }

        internal static bool SupportsReading(Pawn pawn)
        {
            return pawn?.reading != null;
        }

        internal static bool SupportsPlayerSettings(Pawn pawn)
        {
            return pawn?.playerSettings != null;
        }

        internal static bool SupportsInventoryStock(Pawn pawn)
        {
            return pawn?.inventoryStock != null;
        }

        internal static bool ShouldKeepAssignLink(Pawn pawn)
        {
            return SupportsAssign(pawn);
        }

        internal static bool ShouldKeepScheduleLink(Pawn pawn)
        {
            return SupportsSchedule(pawn);
        }

        internal static bool ShouldKeepWorkLink(Pawn pawn)
        {
            return SupportsWork(pawn);
        }

        internal static bool ShouldKeepWeaponsLink(Pawn pawn)
        {
            return SupportsWeapons(pawn);
        }

        internal static List<WorkTypeDef> WorkTypesFor(Pawn pawn)
        {
            if (!SupportsWork(pawn))
            {
                return new List<WorkTypeDef>();
            }

            return DefDatabase<WorkTypeDef>.AllDefsListForReading
                .Where(workType => workType != null && !pawn.WorkTypeIsDisabled(workType))
                .ToList();
        }

        internal static bool TryPawnKey(Pawn pawn, out string key)
        {
            key = null;
            if (pawn == null)
            {
                return false;
            }

            try
            {
                key = pawn.GetUniqueLoadID();
                return !string.IsNullOrEmpty(key);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
