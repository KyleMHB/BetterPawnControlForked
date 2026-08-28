using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace BetterPawnControlForked
{
    [StaticConstructorOnStartup]
    public static class Widget_CombatExtended
    {
        private const string PackageId = "CETeam.CombatExtended";
        private const string DisplayName = "Combat Extended";
        private const string UtilityType = "CombatExtended.Utility_Loadouts";
        private const BindingFlags AllBindings = (BindingFlags)60;

        private static bool initialized;
        private static bool available;
        private static bool failureLogged;
        private static MethodInfo getLoadoutId;
        private static MethodInfo setLoadoutById;

        public static bool CombatExtendedAvailable
        {
            get
            {
                if (!initialized)
                {
                    Initialize();
                }
                return available;
            }
        }

        private static void Initialize()
        {
            initialized = true;
            var mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(item =>
                string.Equals(item.PackageId, PackageId, StringComparison.OrdinalIgnoreCase))
                ?? LoadedModManager.RunningModsListForReading.FirstOrDefault(item => item.Name == DisplayName);
            if (mod == null)
            {
                return;
            }

            try
            {
                var assembly = mod.assemblies.loadedAssemblies.FirstOrDefault(item => item.GetName().Name == "CombatExtended");
                var utility = assembly?.GetType(UtilityType);
                getLoadoutId = utility?.GetMethod("GetLoadoutId", AllBindings);
                setLoadoutById = utility?.GetMethod("SetLoadoutById", AllBindings);
                available = getLoadoutId != null && setLoadoutById != null;
                if (!available)
                {
                    Disable("required type or methods were not found", null);
                    return;
                }

                Log.Message("[BPC] Combat Extended functionality integrated");
            }
            catch (Exception exception)
            {
                Disable("binding failed", exception);
            }
        }

        public static int GetLoadoutId(Pawn pawn)
        {
            if (!CombatExtendedAvailable)
            {
                return -1;
            }

            try
            {
                return (int)getLoadoutId.Invoke(null, new object[] { pawn });
            }
            catch (Exception exception)
            {
                Disable("loadout read failed", exception);
                return -1;
            }
        }

        public static void SetLoadoutById(Pawn pawn, int id)
        {
            if (!CombatExtendedAvailable)
            {
                return;
            }

            try
            {
                setLoadoutById.Invoke(null, new object[] { pawn, id });
            }
            catch (Exception exception)
            {
                Disable("loadout apply failed", exception);
            }
        }

        private static void Disable(string reason, Exception exception)
        {
            available = false;
            if (failureLogged)
            {
                return;
            }

            failureLogged = true;
            Log.Warning("[BPC] Combat Extended integration disabled: " + reason
                + (exception == null ? string.Empty : ". " + exception.GetBaseException().Message));
        }
    }
}
