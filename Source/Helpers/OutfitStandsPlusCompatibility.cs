using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterPawnControl
{
    internal static class OutfitStandsPlusCompatibility
    {
        private const string OutfitStandsPlusPackageId = "khamenman.OutfitStandsPLus";
        private const string StandCompTypeName = "OutfitStandsPlus.ThingComps.OutfitStandsPlusStandComp";
        private const string AssignableCompTypeName = "OutfitStandsPlus.ThingComps.CompAssignableToPawn_OutfitStandsPlusBase";

        private static readonly HashSet<string> ExternalGearFlowJobDefs = new HashSet<string>
        {
            "DPDraftToPosition",
            "GearUpAndGo",
            "Wear",
            "RemoveApparel"
        };

        private static MethodInfo syncApparelPolicyFilterMethod;
        private static bool syncMethodResolved;

        internal static bool Available => Widget_ModsAvailable.OutfitStandsPlusAvailable;

        internal static void TryUseAssignedStandForPolicy(Pawn pawn, ApparelPolicy previousPolicy, ApparelPolicy targetPolicy)
        {
            if (!Available ||
                pawn?.Map?.listerBuildings == null ||
                pawn.apparel == null ||
                pawn.jobs == null ||
                targetPolicy == null ||
                previousPolicy == targetPolicy ||
                HasExternalGearFlowQueued(pawn))
            {
                return;
            }

            Building_OutfitStand stand = FindAssignedStand(pawn);
            if (stand == null || !CanUseStandNow(pawn, stand))
            {
                return;
            }

            SyncApparelPolicyFilter(stand);
            if (!StandHasUsablePolicyApparel(pawn, stand, targetPolicy))
            {
                return;
            }

            Job job = JobMaker.MakeJob(JobDefOf.UseOutfitStand, stand);
            job.playerForced = true;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private static Building_OutfitStand FindAssignedStand(Pawn pawn)
        {
            foreach (Building_OutfitStand stand in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_OutfitStand>())
            {
                CompAssignableToPawn assignmentComp = stand.AllComps
                    .OfType<CompAssignableToPawn>()
                    .FirstOrDefault(comp => comp.GetType().FullName == AssignableCompTypeName);

                if (assignmentComp != null && assignmentComp.AssignedPawns.Contains(pawn))
                {
                    return stand;
                }
            }

            return null;
        }

        private static bool CanUseStandNow(Pawn pawn, Building_OutfitStand stand)
        {
            return stand != null &&
                   !stand.Destroyed &&
                   stand.Spawned &&
                   stand.Map == pawn.Map &&
                   stand.HeldItems != null &&
                   stand.StoreSettings != null &&
                   !pawn.Downed &&
                   pawn.CanReserveAndReach(stand, PathEndMode.InteractionCell, Danger.Deadly);
        }

        private static void SyncApparelPolicyFilter(Building_OutfitStand stand)
        {
            ThingComp standComp = stand.AllComps.FirstOrDefault(comp => comp.GetType().FullName == StandCompTypeName);
            if (standComp == null)
            {
                return;
            }

            try
            {
                if (!syncMethodResolved)
                {
                    syncApparelPolicyFilterMethod = standComp.GetType().GetMethod("SyncApparelPolicyFilter", BindingFlags.Public | BindingFlags.Instance);
                    syncMethodResolved = true;
                }

                syncApparelPolicyFilterMethod?.Invoke(standComp, null);
            }
            catch (Exception ex)
            {
                Log.Warning("[BPC] Outfit Stands Plus policy sync failed; continuing without stand-triggered apparel swap. " + ex.Message);
            }
        }

        private static bool StandHasUsablePolicyApparel(Pawn pawn, Building_OutfitStand stand, ApparelPolicy targetPolicy)
        {
            List<Apparel> standApparel = stand.HeldItems
                .OfType<Apparel>()
                .Where(apparel => IsAllowedByPolicy(targetPolicy, apparel) && CanPawnWearApparel(pawn, apparel))
                .ToList();

            if (standApparel.Count == 0 || !ApparelSetIsInternallyCompatible(pawn, standApparel))
            {
                return false;
            }

            List<Apparel> remainingPawnApparel = pawn.apparel.WornApparel.ToList();
            foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
            {
                List<Apparel> conflicts = standApparel
                    .Where(standItem => !ApparelUtility.CanWearTogether(standItem.def, apparel.def, pawn.RaceProps.body))
                    .ToList();

                if (conflicts.Count == 0)
                {
                    continue;
                }

                if (pawn.apparel.IsLocked(apparel))
                {
                    return false;
                }

                if (!stand.StoreSettings.AllowedToAccept(apparel))
                {
                    return false;
                }

                remainingPawnApparel.Remove(apparel);
            }

            if (remainingPawnApparel.Any(apparel => !IsAllowedByPolicy(targetPolicy, apparel) && !pawn.apparel.IsLocked(apparel)))
            {
                return false;
            }

            return standApparel.All(apparel => remainingPawnApparel.All(worn => ApparelUtility.CanWearTogether(apparel.def, worn.def, pawn.RaceProps.body)));
        }

        private static bool ApparelSetIsInternallyCompatible(Pawn pawn, List<Apparel> apparel)
        {
            for (int i = 0; i < apparel.Count; i++)
            {
                for (int j = i + 1; j < apparel.Count; j++)
                {
                    if (!ApparelUtility.CanWearTogether(apparel[i].def, apparel[j].def, pawn.RaceProps.body))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsAllowedByPolicy(ApparelPolicy policy, Apparel apparel)
        {
            return policy?.filter != null && apparel != null && policy.filter.Allows(apparel);
        }

        private static bool CanPawnWearApparel(Pawn pawn, Apparel apparel)
        {
            return pawn?.RaceProps?.body != null &&
                   apparel?.def?.apparel != null &&
                   !apparel.Destroyed &&
                   ApparelUtility.HasPartsToWear(pawn, apparel.def);
        }

        private static bool HasExternalGearFlowQueued(Pawn pawn)
        {
            if (pawn?.jobs == null)
            {
                return false;
            }

            if (IsExternalGearFlowJob(pawn.jobs.curJob))
            {
                return true;
            }

            return pawn.jobs.jobQueue.Any(queuedJob => IsExternalGearFlowJob(queuedJob.job));
        }

        private static bool IsExternalGearFlowJob(Job job)
        {
            return job?.def?.defName != null && ExternalGearFlowJobDefs.Contains(job.def.defName);
        }

        internal static bool PackageLoaded()
        {
            return LoadedModManager.RunningModsListForReading.Any(mod => string.Equals(mod.PackageId, OutfitStandsPlusPackageId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
