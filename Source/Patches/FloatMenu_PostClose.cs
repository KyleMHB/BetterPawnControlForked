using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlForked
{
    [HarmonyPatch(typeof(FloatMenu), "PostClose")]
    public static class FloatMenu_PostClose
    {
        public static void Postfix()
        {
            if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap?.IsPlayerHome != true)
            {
                return;
            }

            Type windowType = PawnTableSession.ActiveWindowType;
            if (windowType == null)
            {
                return;
            }

            if (windowType == typeof(MainTabWindow_Assign))
            {
                AssignManager.UpdateState(AssignManager.links, AssignManager.Colonists().Where(PawnCompatibility.SupportsAssign).ToList(), AssignManager.GetActivePolicy());
            }
            else if (windowType == typeof(MainTabWindow_Schedule))
            {
                ScheduleManager.UpdateState(ScheduleManager.links, ScheduleManager.Colonists().Where(PawnCompatibility.SupportsSchedule).ToList(), ScheduleManager.GetActivePolicy());
            }
            else if (windowType == typeof(MainTabWindow_Work)
                || windowType.FullName == Widget_ModsAvailable.WORKTAB_MAINTAB
                || windowType.FullName == Widget_ModsAvailable.NUMBERS_MAINTAB)
            {
                WorkManager.SaveCurrentState(WorkManager.Colonists().Where(PawnCompatibility.SupportsWork).ToList());
            }
            else if (windowType == typeof(MainTabWindow_Animals)
                || windowType.FullName == Widget_ModsAvailable.ANIMALTAB_MAINTAB
                || windowType.FullName == Widget_ModsAvailable.NUMBERS_DEFNAME)
            {
                AnimalManager.UpdateState(AnimalManager.links, AnimalManager.Animals().ToList(), AnimalManager.GetActivePolicy());
            }
            else if (windowType == typeof(MainTabWindow_Mechs))
            {
                MechManager.UpdateState(MechManager.links, MechManager.Mechs().ToList(), MechManager.GetActivePolicy());
            }
            else if (windowType.FullName == Widget_ModsAvailable.WEAPONSTAB_MAINTAB && Widget_ModsAvailable.WTBAvailable)
            {
                WeaponsManager.SaveCurrentState(WeaponsManager.Colonists().Where(PawnCompatibility.SupportsWeapons).ToList());
            }
            else if (windowType.FullName == Widget_ModsAvailable.AIROBOTX2_MAINTAB && Widget_ModsAvailable.MiscRobotsAvailable)
            {
                RobotManager.SaveCurrentState(RobotManager.Robots().ToList());
            }
        }
    }
}
