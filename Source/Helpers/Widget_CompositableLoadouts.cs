using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace BetterPawnControlForked
{
    [StaticConstructorOnStartup]
    public static class Widget_CompositableLoadouts
    {
        private const string DisplayNameOnly = "Compositable Loadouts";
        private const string DisplayName = DisplayNameOnly;
        private const string UtilityType = "Inventory.BetterPawnControlForked";
        private const BindingFlags AllBindings = (BindingFlags)60;

        private static bool initialized;
        private static bool available;
        private static bool failureLogged;
        private static MethodInfo getLoadoutId;
        private static MethodInfo setLoadoutById;

        public static bool CompositableLoadoutsAvailable
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
            var mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(item => item.Name == DisplayName);
            if (mod == null)
            {
                return;
            }

            try
            {
                var assembly = mod.assemblies.loadedAssemblies.FirstOrDefault(item => item.GetName().Name == "Inventory");
                var utility = assembly?.GetType(UtilityType);
                getLoadoutId = utility?.GetMethod("GetLoadoutId", AllBindings);
                setLoadoutById = utility?.GetMethod("SetLoadoutById", AllBindings);
                available = getLoadoutId != null && setLoadoutById != null;
                if (!available)
                {
                    Disable("required type or methods were not found", null);
                    return;
                }

                Log.Message("[BPC] Compositable Loadouts functionality integrated");
            }
            catch (Exception exception)
            {
                Disable("binding failed", exception);
            }
        }

        public static int GetLoadoutId(Pawn pawn)
        {
            if (!CompositableLoadoutsAvailable)
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
            if (!CompositableLoadoutsAvailable)
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
            Log.Warning("[BPC] Compositable Loadouts integration disabled: " + reason
                + (exception == null ? string.Empty : ". " + exception.GetBaseException().Message));
        }
    }
}
