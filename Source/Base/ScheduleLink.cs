using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BetterPawnControlForked
{
    public class ScheduleLink : Link, IExposable
    {
        private static readonly Dictionary<ScheduleLink, List<string>> PendingScheduleDefNames = new Dictionary<ScheduleLink, List<string>>();
        //internal int zone = 0;
        internal Pawn colonist = null;
        internal Area area = null;
        internal List<TimeAssignmentDef> schedule;
        //internal int mapId = 0;

        public ScheduleLink() { }

        public ScheduleLink(ScheduleLink link)
        {
            this.zone = link.zone;
            this.colonist = link.colonist;
            this.area = link.area;
            if (link.schedule != null)
            {
                this.schedule = new List<TimeAssignmentDef>(link.schedule);
            }
            this.mapId = link.mapId;
        }

        public ScheduleLink(int zone, Pawn colonist, Area area, List<TimeAssignmentDef> times, int mapId)
        {
            this.zone = zone;
            this.colonist = colonist;
            this.area = area;
            if (times != null)
            {
                this.schedule = new List<TimeAssignmentDef>(times);
            }
            this.mapId = mapId;
        }

        public override string ToString()
        {
            return 
                "Policy:" + zone +
                "  Colonist: " + colonist + 
                "  Area: " + area  + 
                "  MapID: " + mapId;
        }

        /// <summary>
        /// Data for saving/loading
        /// </summary>
        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref zone, "zone", 0, true);
            Scribe_References.Look<Pawn>(ref colonist, "colonist");
            Scribe_References.Look<Area>(ref area, "area");

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<string> scheduleDefNames = GetScheduleDefNames(schedule);
                Scribe_Collections.Look(ref scheduleDefNames, "schedule", LookMode.Value);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<string> scheduleDefNames = null;
                Scribe_Collections.Look(ref scheduleDefNames, "schedule", LookMode.Value);
                PendingScheduleDefNames[this] = scheduleDefNames;
            }
            else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                schedule = ResolveSchedule(TakePendingScheduleDefNames(this));
                RepairSchedule();
            }

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && schedule == null && colonist?.timetable != null)
            {
                //this means the current save does not contain schedule data. So let's start new
                this.schedule = new List<TimeAssignmentDef>(colonist.timetable.times);
            }
            Scribe_Values.Look<int>(ref mapId, "mapId", 0, true);
        }

        internal void RepairSchedule()
        {
            if (schedule == null)
            {
                return;
            }

            for (int hour = 0; hour < schedule.Count; hour++)
            {
                if (schedule[hour] == null)
                {
                    schedule[hour] = DefaultAssignment(hour);
                }
            }

            while (schedule.Count < 24)
            {
                schedule.Add(DefaultAssignment(schedule.Count));
            }
        }

        internal bool TrySetAssignment(int hour, TimeAssignmentDef assignment)
        {
            RepairSchedule();
            if (schedule == null || hour < 0 || hour >= schedule.Count)
            {
                return false;
            }

            schedule[hour] = assignment ?? DefaultAssignment(hour);
            return true;
        }

        internal static TimeAssignmentDef DefaultAssignment(int hour)
        {
            return hour > 5 && hour <= 21 ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep;
        }

        private static List<string> GetScheduleDefNames(List<TimeAssignmentDef> schedule)
        {
            if (schedule == null)
            {
                return null;
            }

            List<string> result = new List<string>(schedule.Count);
            foreach (TimeAssignmentDef assignment in schedule)
            {
                result.Add(assignment?.defName);
            }
            return result;
        }

        private static List<string> TakePendingScheduleDefNames(ScheduleLink scheduleLink)
        {
            if (!PendingScheduleDefNames.TryGetValue(scheduleLink, out var scheduleDefNames))
            {
                return null;
            }

            PendingScheduleDefNames.Remove(scheduleLink);
            return scheduleDefNames;
        }

        private static List<TimeAssignmentDef> ResolveSchedule(List<string> scheduleDefNames)
        {
            if (scheduleDefNames == null)
            {
                return null;
            }

            List<TimeAssignmentDef> result = new List<TimeAssignmentDef>(scheduleDefNames.Count);
            for (int hour = 0; hour < scheduleDefNames.Count; hour++)
            {
                string defName = scheduleDefNames[hour];
                TimeAssignmentDef assignment = string.IsNullOrEmpty(defName)
                    ? null
                    : DefDatabase<TimeAssignmentDef>.GetNamedSilentFail(defName);
                result.Add(assignment ?? DefaultAssignment(hour));
            }
            return result;
        }
    }
}


