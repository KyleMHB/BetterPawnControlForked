using System.Linq;
using HarmonyLib;
using RimWorld;

namespace BetterPawnControlForked
{
    [HarmonyPatch(typeof(MainTabWindow_PawnTable), nameof(MainTabWindow_PawnTable.PostOpen))]
    static class MainTabWindow_PawnTable_OnPostOpen
    {
        static void Postfix(MainTabWindow_PawnTable __instance)
        {
            var windowType = __instance.GetType();
            PawnTableSession.Open(windowType);
            
            if (windowType.Equals(typeof(MainTabWindow_Assign)))
            {
                AssignManager.LoadState(AssignManager.links, AssignManager.Colonists().Where(PawnCompatibility.SupportsAssign).ToList(), AssignManager.GetActivePolicy());
                AssignManager.showPaste = false;
            }

            if (windowType.Equals(typeof(MainTabWindow_Schedule)))
            {

                ScheduleManager.LoadState(ScheduleManager.links, ScheduleManager.Colonists().Where(PawnCompatibility.SupportsSchedule).ToList(), ScheduleManager.GetActivePolicy());
                ScheduleManager.showPaste = false;
            }

            if (windowType.Equals(typeof(MainTabWindow_Work)) || windowType.FullName.Equals(Widget_ModsAvailable.WORKTAB_MAINTAB) || windowType.FullName.Equals(Widget_ModsAvailable.NUMBERS_MAINTAB))
            {
                if (!Widget_ModsAvailable.DisableBPCOnWorkTab)
                {
                    WorkManager.LoadState(WorkManager.links, WorkManager.Colonists().Where(PawnCompatibility.SupportsWork).ToList(), WorkManager.GetActivePolicy());
                    WorkManager.showPaste = false;
                }
            }

            if (windowType.Equals(typeof(MainTabWindow_Animals)) || windowType.FullName.Equals(Widget_ModsAvailable.ANIMALTAB_MAINTAB) || windowType.FullName.Equals(Widget_ModsAvailable.NUMBERS_DEFNAME))
            {
                AnimalManager.LoadState(AnimalManager.links, AnimalManager.Animals().ToList(), AnimalManager.GetActivePolicy());
            }

            if (windowType.Equals(typeof(MainTabWindow_Mechs)))
            {
                MechManager.LoadState(MechManager.links, MechManager.Mechs().ToList(), MechManager.GetActivePolicy());
                MechManager.showPaste = false;
            }
            

            if (windowType.FullName.Equals(Widget_ModsAvailable.WEAPONSTAB_MAINTAB) && Widget_ModsAvailable.WTBAvailable)
            {
                WeaponsManager.LoadState(WeaponsManager.links, WeaponsManager.Colonists().Where(PawnCompatibility.SupportsWeapons).ToList(), WeaponsManager.GetActivePolicy());
                WeaponsManager.showPaste = false;
            }

            if (windowType.FullName.Equals(Widget_ModsAvailable.AIROBOTX2_MAINTAB) && Widget_ModsAvailable.MiscRobotsAvailable)
            {
                RobotManager.LoadState(RobotManager.links, RobotManager.Robots().ToList(), RobotManager.GetActivePolicy());
            }
        }
    }
}

