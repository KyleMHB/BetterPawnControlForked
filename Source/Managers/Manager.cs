using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using BetterPawnControlForked.CoreLogic;

namespace BetterPawnControlForked
{
    abstract class Manager<T> where T : Link
	{
        private static FeatureState<T> Feature => DataStorage.GetFeature<T>();

        internal static List<Policy> policies { get => Feature.policies; set => Feature.policies = value; }
        internal static List<MapActivePolicy> activePolicies { get => Feature.activePolicies; set => Feature.activePolicies = value; }
        internal static List<T> links { get => Feature.links; set => Feature.links = value; }
        internal static bool showPaste { get => Feature.showPaste; set => Feature.showPaste = value; }
        internal static Dictionary<WorkTypeDef, List<WorkGiverDef>> workgivers { get => Feature.workgivers; set => Feature.workgivers = value; }

        static Manager()
        {
            Feature.EnsureInitialized();
        }

        internal static void ForceInit()
        {
            Feature.Reset();
        }

		internal static IEnumerable<Pawn> Colonists()
		{
			try
			{
                return PawnCompatibility.HumanlikePolicyPawns(Find.CurrentMap);
			}
			catch (Exception) 
			{ 
				return new List<Pawn>(); 
			}
        }

        internal static List<WorkGiverDef> GetWorkGivers(WorkTypeDef workType)
        {
            if (workType == null)
            {
                return new List<WorkGiverDef>();
            }

            if (workgivers.TryGetValue(workType, out var result))
                return result;

            var list = DefDatabase<WorkGiverDef>.AllDefsListForReading
                .Where(x => x.workType == workType)
                .ToList();
            workgivers.Add(workType, list);
            
			return list;
        }

        public static bool DirtyPolicy
        {
            get => Feature.dirtyPolicy;
            set => Feature.dirtyPolicy = value;
        }

		internal static Policy GetActivePolicy()
		{
			return GetActivePolicy(Find.CurrentMap.uniqueID);
		}

		internal static void SetActivePolicy(Policy policy)
		{
			SetActivePolicy(Find.CurrentMap.uniqueID, policy);
		}

		internal static Policy GetActivePolicy(int mapId)
		{
			if (activePolicies == null)
			{
				Manager<T>.ForceInit();
			}

			MapActivePolicy mapPolicy = activePolicies.Find(x => x.mapId == mapId);
			if (mapPolicy == null)
			{
				//new map! create default
				mapPolicy = new MapActivePolicy(mapId, policies[0]);
				activePolicies.Add(mapPolicy);
			}
			return mapPolicy.activePolicy;
		}

		internal static Policy GetPolicy(int selected)
		{
			return policies.Find(x => x.id == selected);
		}

        internal static MapActivePolicy GetActiveMap(int mapId)
        {
            if (activePolicies == null)
            {
				//create default
				GetActivePolicy(mapId);
            }

			return activePolicies.Find(x => x.mapId == mapId);
        }

        internal static void SetActivePolicy(int mapId, Policy policy)
		{
			MapActivePolicy map = activePolicies.Find(x => x.mapId == mapId);
			if (map != null)
			{
				map.activePolicy = policy;
			}
			else
			{
				activePolicies.Add(new MapActivePolicy(mapId, policy));
			}
		}

		internal static void MoveLinksToMap(int srcMapId, int dstMapId)
		{
			if (srcMapId == -1)
			{
				//this means there is not last map and nothing should be done.
				Log.Warning("[BPC] Couldn't copy settings to new map since last map does not exit");
				return;
			}

			foreach (T link in links.Where(link => link != null && link.mapId == srcMapId))
            {
                link.mapId = dstMapId;
            }

            var transferPlan = MapStateTransfer.Plan(activePolicies.Select(item => item.mapId).ToList(), srcMapId, dstMapId);
            if (!transferPlan.ShouldTransfer)
            {
                return;
            }

            var source = activePolicies[transferPlan.SourceSelectionIndex];
            for (var index = transferPlan.DestinationSelectionIndexes.Count - 1; index >= 0; index--)
            {
                activePolicies.RemoveAt(transferPlan.DestinationSelectionIndexes[index]);
            }
            source.mapId = dstMapId;
		}

        internal static void ProcessNewMapTransition(Map newMap)
        {
            if (newMap == null || !newMap.IsPlayerHome || LastMapManager.lastMapId < 0)
            {
                return;
            }

            bool anotherPlayerHomeHasState = Find.Maps.Any(map =>
                map != null && map != newMap && map.IsPlayerHome
                && activePolicies.Any(selection => selection.mapId == map.uniqueID));
            if (anotherPlayerHomeHasState || !activePolicies.Any(selection => selection.mapId == LastMapManager.lastMapId))
            {
                return;
            }

            MoveLinksToMap(LastMapManager.lastMapId, newMap.uniqueID);
        }

		internal static bool FoodPolicyExists(FoodPolicy foodPolicy)
		{
			if (foodPolicy == null || Current.Game?.foodRestrictionDatabase == null)
			{
				return false;
			}

			foreach (FoodPolicy food in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
			{
				if (food.Equals(foodPolicy))
				{
					return true;
				}
			}
			return false;
		}

		internal static FoodPolicy _defaultFoodPolicy { get => Feature.defaultFoodPolicy; set => Feature.defaultFoodPolicy = value; }
		internal static FoodPolicy DefaultFoodPolicy
		{
			get
			{
				if (_defaultFoodPolicy == null)
				{
					_defaultFoodPolicy = Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
				}
				return _defaultFoodPolicy;
			}

			set
			{
				_defaultFoodPolicy = value;
			}
		}

		internal static ReadingPolicy _defaultReadingPolicy { get => Feature.defaultReadingPolicy; set => Feature.defaultReadingPolicy = value; }
		internal static ReadingPolicy DefaultReadingPolicy
		{
			get
			{
				if (_defaultReadingPolicy == null)
				{
					_defaultReadingPolicy = Current.Game.readingPolicyDatabase.DefaultReadingPolicy();
				}
				return _defaultReadingPolicy;
			}

			set
			{
				_defaultReadingPolicy = value;
			}
		}

        internal static MedicalCareCategory DefaultMedsPolicy
        {
            get
            {
                return Current.Game.playSettings.defaultCareForColonist;
            }

            set
            {
                Current.Game.playSettings.defaultCareForColonist = value;
            }
        }
    }
}



