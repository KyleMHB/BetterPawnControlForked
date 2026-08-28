using HarmonyLib;
using Verse;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch(typeof(MapComponentUtility), nameof(MapComponent.MapGenerated))]
    static class MapGenerated
    {
        static void Postfix(Map map)
        {
            AssignManager.ProcessNewMap(map);
            ScheduleManager.ProcessNewMap(map);
            WorkManager.ProcessNewMap(map);
            AnimalManager.ProcessNewMap(map);
            MechManager.ProcessNewMap(map);
            WeaponsManager.ProcessNewMap(map);
            RobotManager.ProcessNewMap(map);
        }
    }
}


