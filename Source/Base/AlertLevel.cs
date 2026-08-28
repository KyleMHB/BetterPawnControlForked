using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using BetterPawnControlForked.CoreLogic;

namespace BetterPawnControlForked
{
    public class AlertLevel : IExposable
    {
        private static readonly HashSet<string> LoggedFallbacks = new HashSet<string>();

        internal Dictionary<Resources.Type, Policy> settings;
        internal Dictionary<Resources.Type, int> policyIds;
        internal int level;

        public AlertLevel()
        {
        }

        public AlertLevel(int level, Dictionary<Resources.Type, Policy> settings)
        {
            this.level = level;
            this.settings = settings ?? new Dictionary<Resources.Type, Policy>();
            RefreshPolicyIds();
        }

        public override string ToString()
        {
            return "Level: " + level + " \n  Settings: " + settings.ToStringFullContents();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref level, "level", 0, true);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                RefreshPolicyIds();
            }

            List<Resources.Type> keys = null;
            List<int> values = null;
            Scribe_Collections.Look(ref policyIds, "policyIds", LookMode.Value, LookMode.Value, ref keys, ref values);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Dictionary<Resources.Type, Policy> legacySettings = null;
                List<Resources.Type> legacyKeys = null;
                List<Policy> legacyValues = null;
                Scribe_Collections.Look(ref legacySettings, "settings", LookMode.Value, LookMode.Deep, ref legacyKeys, ref legacyValues);

                policyIds = policyIds ?? new Dictionary<Resources.Type, int>();
                if (legacySettings != null)
                {
                    foreach (var pair in legacySettings.Where(pair => pair.Value != null))
                    {
                        policyIds[pair.Key] = pair.Value.id;
                    }
                }
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ResolvePolicies();
            }
        }

        internal void RefreshPolicyIds()
        {
            policyIds = policyIds ?? new Dictionary<Resources.Type, int>();
            if (settings == null)
            {
                return;
            }

            foreach (var pair in settings.Where(pair => pair.Value != null))
            {
                policyIds[pair.Key] = pair.Value.id;
            }
        }

        internal void ResolvePolicies()
        {
            policyIds = policyIds ?? new Dictionary<Resources.Type, int>();
            settings = settings ?? new Dictionary<Resources.Type, Policy>();

            foreach (var pair in policyIds.ToList())
            {
                settings[pair.Key] = ResolvePolicy(pair.Key, pair.Value);
                policyIds[pair.Key] = settings[pair.Key]?.id ?? 0;
            }
        }

        internal Policy ResolvePolicy(Resources.Type type, int requestedId)
        {
            var policies = PoliciesFor(type);
            var resolvedId = PolicyIdResolver.Resolve(requestedId, policies.Select(item => item.id));
            var policy = policies.FirstOrDefault(item => item.id == resolvedId);

            if (resolvedId != requestedId)
            {
                var key = level + ":" + type + ":" + requestedId;
                if (LoggedFallbacks.Add(key))
                {
                    Log.Warning("[BPC] Emergency policy " + requestedId + " for " + type + " no longer exists; using policy 0.");
                }
            }

            return policy;
        }

        private static List<Policy> PoliciesFor(Resources.Type type)
        {
            switch (type)
            {
                case Resources.Type.work: return WorkManager.policies;
                case Resources.Type.restrict: return ScheduleManager.policies;
                case Resources.Type.assign: return AssignManager.policies;
                case Resources.Type.animal: return AnimalManager.policies;
                case Resources.Type.mech: return MechManager.policies;
                case Resources.Type.weapons: return WeaponsManager.policies;
                case Resources.Type.robots: return RobotManager.policies;
                default: return AssignManager.policies;
            }
        }
    }
}
