using HarmonyLib;
using Verse;

namespace BetterPawnControlForked.Patches
{
    [HarmonyPatch(typeof(MapComponentUtility), nameof(MapComponentUtility.MapRemoved))]
    static class MapComponent_OnRemove
    {
        static void Postfix(Map map)
        {
            LastMapManager.lastMapId = map.uniqueID;

            AssignManager.CleanRemovedMaps(map);
            ScheduleManager.CleanRemovedMaps(map);
            WorkManager.CleanRemovedMaps(map);
            AnimalManager.CleanRemovedMaps(map);
            MechManager.CleanRemovedMaps(map);
            WeaponsManager.CleanRemovedMaps(map);
            RobotManager.CleanRemovedMaps(map);
        }
    }
}


