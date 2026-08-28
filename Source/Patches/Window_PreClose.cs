using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch(typeof(Window), nameof(Window.PreClose))]
    static class Window_PreClose
    {        
        static void Postfix(Window __instance)
        {
            if (__instance.GetType().Equals(typeof(MainTabWindow_Assign)) || __instance.GetType().FullName.Equals(Widget_ModsAvailable.WEAPONSTAB_MAINTAB)) 
            {
                AssignManager.SaveCurrentState(AssignManager.Colonists().Where(PawnCompatibility.SupportsAssign).ToList());
                AssignManager.LinksCleanUp();
            }

            if (__instance.GetType().Equals(typeof(MainTabWindow_Work)) || (__instance.GetType().FullName.Equals(Widget_ModsAvailable.WORKTAB_MAINTAB) && !Widget_ModsAvailable.DisableBPCOnWorkTab) || __instance.GetType().FullName.Equals(Widget_ModsAvailable.NUMBERS_MAINTAB))
            {
                WorkManager.SaveCurrentState(WorkManager.Colonists().Where(PawnCompatibility.SupportsWork).ToList());
                WorkManager.LinksCleanUp();
                Widget_WorkTab.ClearCache();
            }

            if (__instance.GetType().Equals(typeof(MainTabWindow_Schedule)))
            {
                ScheduleManager.SaveCurrentState(ScheduleManager.Colonists().Where(PawnCompatibility.SupportsSchedule).ToList());
                ScheduleManager.LinksCleanUp();
            }

            if (__instance.GetType().Equals(typeof(MainTabWindow_Animals)) || __instance.GetType().FullName.Equals(Widget_ModsAvailable.ANIMALTAB_MAINTAB) || __instance.GetType().FullName.Equals(Widget_ModsAvailable.NUMBERS_DEFNAME))
            {
                AnimalManager.SaveCurrentState(AnimalManager.Animals().ToList());
                AnimalManager.LinksCleanUp();
            }

            if (__instance.GetType().Equals(typeof(MainTabWindow_Mechs)))
            {
                MechManager.SaveCurrentState(MechManager.Mechs().ToList());
                MechManager.LinksCleanUp();
            }

            if (__instance.GetType().FullName.Equals(Widget_ModsAvailable.WEAPONSTAB_MAINTAB) && Widget_ModsAvailable.WTBAvailable)
            {
                WeaponsManager.SaveCurrentState(WeaponsManager.Colonists().Where(PawnCompatibility.SupportsWeapons).ToList());
                WeaponsManager.LinksCleanUp();
            }

            if (__instance.GetType().FullName.Equals(Widget_ModsAvailable.AIROBOTX2_MAINTAB) && Widget_ModsAvailable.MiscRobotsAvailable)
            {
                RobotManager.SaveCurrentState(RobotManager.Robots().ToList());
                RobotManager.LinksCleanUp();
            }

            PawnTableSession.Close(__instance.GetType());
        }
    }
}


