using System.Linq;
using HarmonyLib;
using System;
using Verse;
using static BetterPawnControlForked.BetterPawnControlForkedMod;

namespace BetterPawnControlForked
{
    [StaticConstructorOnStartup]
    public static class Widget_ModsAvailable
    {
        private const string WORKTAB = "Work Tab";
        private const string WORKTAB_PACKAGE_ID = "Fluffy.WorkTab";
        private const string ANIMALTAB= "Animal Tab";
        private const string ANIMALTAB_PACKAGE_ID = "Fluffy.AnimalTab";
        private const string CSL = "Children, school and learning";
        private const string AAF = "Assign Animal Food";
        private const string WTB = "[1001]Weapons Tab Reborn";
        private const string MISCROBOTS = "Misc. Robots";
        private const string DEFENSIVE_POSITIONS_PACKAGE_ID = "GonDragon.DefensivePositions";

        internal const string WORKTAB_MAINTAB = "WorkTab.MainTabWindow_WorkTab";
        internal const string ANIMALTAB_MAINTAB = "AnimalTab.MainTabWindow_Animals";
        internal const string NUMBERS_MAINTAB = "Numbers.MainTabWindow_Numbers";
        internal const string NUMBERS_DEFNAME = "Numbers.MainTabWindow_NumbersAnimals";       
        internal const string WEAPONSTAB_MAINTAB = "WeaponsTabReborn.MainTabWindow_Weapons";
        internal const string AIROBOTX2_MAINTAB = "AIRobot.X2_MainTabWindow_Robots";

        static Widget_ModsAvailable() 
        {
            if (LoadedModManager.RunningModsListForReading.Any(mod => string.Equals(mod.PackageId, "VouLT.BetterPawnControl", StringComparison.OrdinalIgnoreCase)))
            {
                Log.Error("[BPC] Original Better Pawn Control and Better Pawn Control Forked are both active. Fork patches are disabled to prevent duplicate behavior.");
                return;
            }

            var harmony = new Harmony("KyleMHB.BetterPawnControlForked");
            harmony.PatchAll();
        }

        public static bool AnimalTabAvailable
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mod => string.Equals(mod.PackageId, ANIMALTAB_PACKAGE_ID, StringComparison.OrdinalIgnoreCase) || mod.Name == ANIMALTAB);
            }
        }

        public static bool WorkTabAvailable
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mod => string.Equals(mod.PackageId, WORKTAB_PACKAGE_ID, StringComparison.OrdinalIgnoreCase) || mod.Name.StartsWith(WORKTAB));
            }
        }

        public static bool DisableBPCOnWorkTab
        {
            get
            {
                return WorkTabAvailable && Settings.disableBPCOnWorkTab;
            }
        }

        public static bool DisableBPCWorkTabInnerPriorities
        {
            get
            {
                return WorkTabAvailable && Settings.disableBPCWorkTabInnerPriorities;
            }
        }

        public static bool CSLAvailable
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mod => mod.Name == CSL);
            }
        }

        public static bool AAFAvailable
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mod => mod.Name == AAF);
            }
        }

        public static bool WTBAvailable
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mod => mod.Name == WTB) && Widget_WeaponsTabReborn.Integrated(); 
            }
        }

        public static bool CEAvailable
        {
            get
            {
                return Widget_CombatExtended.CombatExtendedAvailable;
            }
        }

        public static bool MiscRobotsAvailable
        {
            get
            {
                return LoadedModManager.RunningMods.Any(mod => mod.Name == MISCROBOTS);
            }
        }

        public static bool CompositableAvailable
        {
            get
            {
                return Widget_CompositableLoadouts.CompositableLoadoutsAvailable;;
            }
        }

        public static bool DefensivePositionsAvailable
        {
            get
            {
                return LoadedModManager.RunningModsListForReading.Any(mod => string.Equals(mod.PackageId, DEFENSIVE_POSITIONS_PACKAGE_ID, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static bool OutfitStandsPlusAvailable
        {
            get
            {
                return OutfitStandsPlusCompatibility.PackageLoaded();
            }
        }
    }
}



