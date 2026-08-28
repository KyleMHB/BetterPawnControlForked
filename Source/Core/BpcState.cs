using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BetterPawnControlForked
{
    internal sealed class FeatureState<T> where T : Link
    {
        internal List<Policy> policies;
        internal List<MapActivePolicy> activePolicies;
        internal List<T> links;
        internal List<T> clipboard;
        internal bool showPaste;
        internal bool dirtyPolicy;
        internal Dictionary<WorkTypeDef, List<WorkGiverDef>> workgivers;
        internal FoodPolicy defaultFoodPolicy;
        internal ReadingPolicy defaultReadingPolicy;

        internal FeatureState()
        {
            Reset();
        }

        internal void Reset()
        {
            policies = new List<Policy>();
            activePolicies = new List<MapActivePolicy>();
            links = new List<T>();
            clipboard = new List<T>();
            workgivers = new Dictionary<WorkTypeDef, List<WorkGiverDef>>();
            showPaste = false;
            dirtyPolicy = false;
            EnsureInitialized();
        }

        internal void EnsureInitialized()
        {
            policies = policies ?? new List<Policy>();
            activePolicies = activePolicies ?? new List<MapActivePolicy>();
            links = links ?? new List<T>();
            clipboard = clipboard ?? new List<T>();
            workgivers = workgivers ?? new Dictionary<WorkTypeDef, List<WorkGiverDef>>();

            if (policies.Count == 0)
            {
                policies.Add(new Policy(0, "BPC.Auto".Translate()));
            }

            if (activePolicies.Count == 0)
            {
                activePolicies.Add(new MapActivePolicy(0, policies[0]));
            }
        }
    }

    internal sealed class BpcState
    {
        internal FeatureState<AssignLink> assign = new FeatureState<AssignLink>();
        internal FeatureState<AnimalLink> animal = new FeatureState<AnimalLink>();
        internal FeatureState<ScheduleLink> schedule = new FeatureState<ScheduleLink>();
        internal FeatureState<WorkLink> work = new FeatureState<WorkLink>();
        internal FeatureState<MechLink> mech = new FeatureState<MechLink>();
        internal FeatureState<WeaponsLink> weapons = new FeatureState<WeaponsLink>();
        internal FeatureState<RobotLink> robot = new FeatureState<RobotLink>();

        internal ApparelPolicy defaultOutfit;
        internal DrugPolicy defaultDrugPolicy;
        internal FoodPolicy defaultPrisonerFoodPolicy;
        internal ApparelPolicy defaultSlaveOutfit;
        internal FoodPolicy defaultSlaveFoodPolicy;
        internal DrugPolicy defaultSlaveDrugPolicy;
        internal ReadingPolicy defaultSlaveReadingPolicy;
        internal int defaultLoadoutId;
        internal int alertLevel;
        internal List<AlertLevel> alertLevels = new List<AlertLevel>();
        internal int lastMapId = -1;

        internal FeatureState<T> Feature<T>() where T : Link
        {
            if (typeof(T) == typeof(AssignLink)) return (FeatureState<T>)(object)assign;
            if (typeof(T) == typeof(AnimalLink)) return (FeatureState<T>)(object)animal;
            if (typeof(T) == typeof(ScheduleLink)) return (FeatureState<T>)(object)schedule;
            if (typeof(T) == typeof(WorkLink)) return (FeatureState<T>)(object)work;
            if (typeof(T) == typeof(MechLink)) return (FeatureState<T>)(object)mech;
            if (typeof(T) == typeof(WeaponsLink)) return (FeatureState<T>)(object)weapons;
            if (typeof(T) == typeof(RobotLink)) return (FeatureState<T>)(object)robot;
            throw new InvalidOperationException("Unsupported BPC feature link type: " + typeof(T).FullName);
        }

        internal void EnsureInitialized()
        {
            assign.EnsureInitialized();
            animal.EnsureInitialized();
            schedule.EnsureInitialized();
            work.EnsureInitialized();
            mech.EnsureInitialized();
            weapons.EnsureInitialized();
            robot.EnsureInitialized();
            alertLevels = alertLevels ?? new List<AlertLevel>();
        }
    }
}
