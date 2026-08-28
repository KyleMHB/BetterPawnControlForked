using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterPawnControlForked
{
    public class WorkLink : Link, IExposable
    {
        internal Pawn colonist;
        internal Dictionary<WorkTypeDef, int> settings;
        internal Dictionary<WorkGiverDef, List<int>> settingsInner;
        internal Dictionary<string, int> settingsByDefName;
        internal Dictionary<string, string> settingsInnerByDefName;

        public WorkLink()
        {
        }

        public WorkLink(WorkLink link)
        {
            zone = link.zone;
            colonist = link.colonist;
            mapId = link.mapId;
            settings = link.settings != null
                ? new Dictionary<WorkTypeDef, int>(link.settings)
                : new Dictionary<WorkTypeDef, int>();
            settingsInner = link.settingsInner != null
                ? link.settingsInner.ToDictionary(pair => pair.Key, pair => new List<int>(pair.Value))
                : new Dictionary<WorkGiverDef, List<int>>();
            settingsByDefName = link.settingsByDefName != null
                ? new Dictionary<string, int>(link.settingsByDefName)
                : new Dictionary<string, int>();
            settingsInnerByDefName = link.settingsInnerByDefName != null
                ? new Dictionary<string, string>(link.settingsInnerByDefName)
                : new Dictionary<string, string>();
            RefreshDefNames();
        }

        public WorkLink(int zone, Pawn colonist, Dictionary<WorkTypeDef, int> settings, Dictionary<WorkGiverDef, List<int>> settingsInner, int mapId)
        {
            this.zone = zone;
            this.colonist = colonist;
            this.settings = settings ?? new Dictionary<WorkTypeDef, int>();
            this.settingsInner = settingsInner ?? new Dictionary<WorkGiverDef, List<int>>();
            this.mapId = mapId;
            RefreshDefNames();
        }

        public override string ToString()
        {
            return "Policy:" + zone + "  Pawn: " + colonist + "  MapID: " + mapId;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref zone, "zone", 0, true);
            Scribe_References.Look(ref colonist, "colonist");
            Scribe_Values.Look(ref mapId, "mapId", 0, true);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                RefreshDefNames();
            }

            List<string> workKeys = null;
            List<int> workValues = null;
            Scribe_Collections.Look(ref settingsByDefName, "settingsByDefName", LookMode.Value, LookMode.Value, ref workKeys, ref workValues);

            List<string> giverKeys = null;
            List<string> giverValues = null;
            Scribe_Collections.Look(ref settingsInnerByDefName, "settingsInnerByDefName", LookMode.Value, LookMode.Value, ref giverKeys, ref giverValues);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                LoadLegacyDefKeyedData();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ResolveAvailableDefs();
            }
        }

        private void LoadLegacyDefKeyedData()
        {
            var legacySettings = new Dictionary<WorkTypeDef, int>();
            List<WorkTypeDef> legacyWorkKeys = null;
            List<int> legacyWorkValues = null;
            Scribe_Collections.Look(ref legacySettings, "settings", LookMode.Def, LookMode.Value, ref legacyWorkKeys, ref legacyWorkValues);

            var legacyInner = new Dictionary<WorkGiverDef, string>();
            List<WorkGiverDef> legacyInnerKeys = null;
            List<string> legacyInnerValues = null;
            Scribe_Collections.Look(ref legacyInner, "settingsInner", LookMode.Def, LookMode.Value, ref legacyInnerKeys, ref legacyInnerValues);

            settingsByDefName = settingsByDefName ?? new Dictionary<string, int>();
            settingsInnerByDefName = settingsInnerByDefName ?? new Dictionary<string, string>();
            legacySettings = legacySettings ?? new Dictionary<WorkTypeDef, int>();
            legacyInner = legacyInner ?? new Dictionary<WorkGiverDef, string>();

            foreach (var pair in legacySettings.Where(pair => pair.Key != null))
            {
                settingsByDefName[pair.Key.defName] = pair.Value;
            }

            foreach (var pair in legacyInner.Where(pair => pair.Key != null && pair.Value != null))
            {
                settingsInnerByDefName[pair.Key.defName] = pair.Value;
            }
        }

        internal void RefreshDefNames()
        {
            settingsByDefName = settingsByDefName ?? new Dictionary<string, int>();
            settingsInnerByDefName = settingsInnerByDefName ?? new Dictionary<string, string>();

            if (settings != null)
            {
                foreach (var pair in settings.Where(pair => pair.Key != null))
                {
                    settingsByDefName[pair.Key.defName] = pair.Value;
                }
            }

            if (settingsInner != null)
            {
                foreach (var pair in settingsInner.Where(pair => pair.Key != null && pair.Value != null))
                {
                    settingsInnerByDefName[pair.Key.defName] = string.Join(",", pair.Value);
                }
            }
        }

        internal void ResolveAvailableDefs()
        {
            settingsByDefName = settingsByDefName ?? new Dictionary<string, int>();
            settingsInnerByDefName = settingsInnerByDefName ?? new Dictionary<string, string>();
            settings = new Dictionary<WorkTypeDef, int>();
            settingsInner = new Dictionary<WorkGiverDef, List<int>>();

            foreach (var pair in settingsByDefName)
            {
                var workType = DefDatabase<WorkTypeDef>.GetNamedSilentFail(pair.Key);
                if (workType != null)
                {
                    settings[workType] = pair.Value;
                }
            }

            foreach (var pair in settingsInnerByDefName)
            {
                var workGiver = DefDatabase<WorkGiverDef>.GetNamedSilentFail(pair.Key);
                if (workGiver == null)
                {
                    continue;
                }

                settingsInner[workGiver] = (pair.Value ?? string.Empty)
                    .Split(',')
                    .Where(value => !string.IsNullOrEmpty(value))
                    .Select(value => int.TryParse(value, out var priority) ? priority : 3)
                    .ToList();
            }
        }
    }
}
