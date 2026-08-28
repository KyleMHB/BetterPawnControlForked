using System.Collections.Generic;
using System.Linq;
using BetterPawnControlForked.Helpers;
using RimWorld;
using Verse;
using static BetterPawnControlForked.BetterPawnControlForkedMod;

namespace BetterPawnControlForked
{
    [StaticConstructorOnStartup]
    class AssignManager : Manager<AssignLink>
    {
        internal static List<AssignLink> clipboard => DataStorage.GetFeature<AssignLink>().clipboard;


        internal static ApparelPolicy _defaultOutfit { get => DataStorage.State.defaultOutfit; set => DataStorage.State.defaultOutfit = value; }
        internal static ApparelPolicy DefaultOutfit
        {
            get
            {
                if (_defaultOutfit == null)
                {

                    _defaultOutfit = Current.Game.outfitDatabase.DefaultOutfit();
                }
                return _defaultOutfit;
            }

            set
            {
                _defaultOutfit = value;
            }
        }

        internal static DrugPolicy _defaultDrugPolicy { get => DataStorage.State.defaultDrugPolicy; set => DataStorage.State.defaultDrugPolicy = value; }
        internal static DrugPolicy DefaultDrugPolicy
        {
            get
            {
                if (_defaultDrugPolicy == null)
                {
                    _defaultDrugPolicy = Current.Game.drugPolicyDatabase.DefaultDrugPolicy();
                }
                return _defaultDrugPolicy;
            }

            set
            {
                _defaultDrugPolicy = value;
            }
        }


        internal static FoodPolicy _defaultPrisonerFoodPolicy { get => DataStorage.State.defaultPrisonerFoodPolicy; set => DataStorage.State.defaultPrisonerFoodPolicy = value; }
        internal static FoodPolicy DefaultPrisonerFoodPolicy
        {
            get
            {
                if (_defaultPrisonerFoodPolicy == null)
                {
                    _defaultPrisonerFoodPolicy = Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
                }
                return _defaultPrisonerFoodPolicy;
            }

            set
            {
                _defaultPrisonerFoodPolicy = value;
            }
        }

        internal static MedicalCareCategory DefaultPrisonerMedicinePolicy
        {
            get
            {
                return Current.Game.playSettings.defaultCareForPrisoner;
            }

            set
            {
                Current.Game.playSettings.defaultCareForPrisoner = value;
            }
        }

        internal static MedicalCareCategory DefaultSlaveMedicinePolicy
        {
            get
            {
                return Current.Game.playSettings.defaultCareForSlave;
            }

            set
            {
                Current.Game.playSettings.defaultCareForSlave = value;
            }
        }

        internal static ApparelPolicy _defaultSlaveOutfit { get => DataStorage.State.defaultSlaveOutfit; set => DataStorage.State.defaultSlaveOutfit = value; }
        internal static ApparelPolicy DefaultSlaveOutfit
        {
            get
            {
                if (_defaultSlaveOutfit == null)
                {

                    _defaultSlaveOutfit = Current.Game.outfitDatabase.DefaultOutfit();
                }
                return _defaultSlaveOutfit;
            }

            set
            {
                _defaultSlaveOutfit = value;
            }
        }

        internal static FoodPolicy _defaultSlaveFoodPolicy { get => DataStorage.State.defaultSlaveFoodPolicy; set => DataStorage.State.defaultSlaveFoodPolicy = value; }
        internal static FoodPolicy DefaultSlaveFoodPolicy
        {
            get
            {
                if (_defaultSlaveFoodPolicy == null)
                {
                    _defaultSlaveFoodPolicy = DefaultFoodPolicy;
                }
                return _defaultSlaveFoodPolicy;
            }

            set
            {
                _defaultSlaveFoodPolicy = value;
            }
        }

        internal static DrugPolicy _defaultSlaveDrugPolicy { get => DataStorage.State.defaultSlaveDrugPolicy; set => DataStorage.State.defaultSlaveDrugPolicy = value; }
        internal static DrugPolicy DefaultSlaveDrugPolicy
        {
            get
            {
                if (_defaultSlaveDrugPolicy == null)
                {
                    _defaultSlaveDrugPolicy = Current.Game.drugPolicyDatabase.DefaultDrugPolicy();
                }
                return _defaultSlaveDrugPolicy;
            }

            set
            {
                _defaultSlaveDrugPolicy = value;
            }
        }

        internal static ReadingPolicy _defaultSlaveReadingPolicy { get => DataStorage.State.defaultSlaveReadingPolicy; set => DataStorage.State.defaultSlaveReadingPolicy = value; }
        internal static ReadingPolicy DefaultSlaveReadingPolicy
        {
            get
            {
                if (_defaultSlaveReadingPolicy == null)
                {
                    _defaultSlaveReadingPolicy = Current.Game.readingPolicyDatabase.DefaultReadingPolicy();
                }
                return _defaultSlaveReadingPolicy;
            }

            set
            {
                _defaultSlaveReadingPolicy = value;
            }
        }

        internal static void DeletePolicy(Policy policy)
        {
            //delete if not default AssignPolicy
            if (policy != null && policy.id > 0)
            {
                links.RemoveAll(x => x.zone == policy.id);
                policies.Remove(policy);
                int mapId = Find.CurrentMap.uniqueID;
                foreach (MapActivePolicy m in activePolicies)
                {
                    if (m.activePolicy.id == policy.id)
                    {
                        m.activePolicy = policies[0];
                        DirtyPolicy = true;
                    }
                }
            }
        }

        internal static void DeleteLinksInMap(int mapId)
        {
            links.RemoveAll(x => x.mapId == mapId);
        }

        internal static void DeleteMap(MapActivePolicy map)
        {
            activePolicies.Remove(map);
        }

        internal static void SaveCurrentState(List<Pawn> pawns)
        {
            int currentMap = Find.CurrentMap.uniqueID;
            int activePolicyId = AssignManager.GetActivePolicy().id;
            //Save current state
            foreach (Pawn p in pawns)
            {
                if (!PawnCompatibility.SupportsAssign(p))
                {
                    continue;
                }

                try
                {                
                    //find colonist on the current zone in the current map
                    AssignLink link = AssignManager.links.Find(
                        x => x != null && x.colonist != null  && p!= null && PawnCompatibility.SamePawn(p, x.colonist) &&
                        x.zone == activePolicyId &&
                        x.mapId == currentMap);

                    if (link != null)
                    {
                        //colonist found! save 
                        if (p.outfits != null)
                        {
                            link.outfit = p.outfits.CurrentApparelPolicy;
                        }
                        if (p.drugs != null)
                        {
                            link.drugPolicy = p.drugs.CurrentPolicy;
                        }
                        if (p.playerSettings != null)
                        {
                            link.hostilityResponse = p.playerSettings.hostilityResponse;
                            link.medicinePolicy = p.playerSettings.medCare;
                        }
                        if (p.foodRestriction != null)
                        {
                            link.foodPolicy = p.foodRestriction.CurrentFoodPolicy;
                        }
                        if (p.reading != null)
                        {
                            link.readingPolicy = p.reading.CurrentPolicy;
                        }
                        if (PawnCompatibility.SupportsInventoryStock(p))
                        {
                            link.SetInventoryStockForMedicine(p.inventoryStock);
                        }

                        if (PawnCompatibility.SupportsWeapons(p) && Widget_CombatExtended.CombatExtendedAvailable)
                        {
                            link.loadoutId = Widget_CombatExtended.GetLoadoutId(p);
                        }
                        if (PawnCompatibility.SupportsWeapons(p) && Widget_CompositableLoadouts.CompositableLoadoutsAvailable)
                        {
                            link.compositableState = Widget_CompositableLoadouts.GetLoadoutId(p);
                        }
                    }
                    else
                    {
                        //colonist not found. So add it to the AssignLink list
                        int loadoutId = 0;
                        if (PawnCompatibility.SupportsWeapons(p) && Widget_CombatExtended.CombatExtendedAvailable)
                        {
                            loadoutId = Widget_CombatExtended.GetLoadoutId(p);
                        }
                        int compositableState = -1;
                        if (PawnCompatibility.SupportsWeapons(p) && Widget_CompositableLoadouts.CompositableLoadoutsAvailable)
                        {
                            compositableState = Widget_CompositableLoadouts.GetLoadoutId(p);
                        }

                        ApparelPolicy outfit = p.outfits?.CurrentApparelPolicy;
                        if (outfit != null && Current.Game?.outfitDatabase != null && outfit == Current.Game.outfitDatabase.DefaultOutfit())
                        {
                            outfit = AssignManager.DefaultOutfit;
                        }

                        DrugPolicy drug = p.drugs?.CurrentPolicy;
                        if (drug != null && Current.Game?.drugPolicyDatabase != null && drug == Current.Game.drugPolicyDatabase.DefaultDrugPolicy())
                        {
                            drug = AssignManager.DefaultDrugPolicy;
                        }

                        FoodPolicy food = p.foodRestriction?.CurrentFoodPolicy;
                        if (food != null && Current.Game?.foodRestrictionDatabase != null && food == Current.Game.foodRestrictionDatabase.DefaultFoodRestriction())
                        {
                            food = AssignManager.DefaultFoodPolicy;
                        }

                        ReadingPolicy reading = p.reading?.CurrentPolicy;
                        if (reading != null && Current.Game?.readingPolicyDatabase != null && reading == Current.Game.readingPolicyDatabase.DefaultReadingPolicy())
                        {
                            reading = AssignManager.DefaultReadingPolicy;
                        }

                        link = new AssignLink(
                                activePolicyId,
                                p,
                                outfit,
                                food,
                                drug,
                                reading,
                                p.playerSettings?.hostilityResponse ?? HostilityResponseMode.Flee,
                                p.playerSettings?.medCare ?? MedicalCareCategory.Best,
                                loadoutId,
                                compositableState,
                                currentMap);
                        AssignManager.links.Add(link);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Warning("[BPC] Failed to save assignment state for pawn " + p.ToStringSafe() + ". " + ex.Message);
                }
            }
        }

        internal static void CleanDeadColonists(Pawn pawn)
        {
            AssignManager.links.RemoveAll(x => x.colonist == pawn);
        }

        internal static void LinksCleanUp()
        {
            for (int i = AssignManager.links.Count - 1; i >= 0; i--)
            {
                if (AssignManager.links[i].colonist == null || !PawnCompatibility.ShouldKeepAssignLink(AssignManager.links[i].colonist))
                {
                    AssignManager.links.RemoveAt(i);
                }
            }
        }

        internal static bool ActivePoliciesContainsValidMap()
        {
            bool containsValidMap = false;
            foreach (Map map in Find.Maps)
            {
                if (AssignManager.activePolicies.Any(x => x.mapId == map.uniqueID))
                {
                    containsValidMap = true;
                    break;
                }
            }
            return containsValidMap;
        }

        internal static void CleanRemovedMaps(Map map)
        {
            //for (int i = 0; i < AssignManager.activePolicies.Count; i++)
            //{
            //    MapActivePolicy map = AssignManager.activePolicies[i];
            //    if (!Find.Maps.Any(x => x.uniqueID == map.mapId))
            //    {
            //        Log.Message("Find.Maps.Count: " + Find.Maps.Count);
            //        Log.Message("AssignManager.ActivePoliciesContainsValidMap(): " + AssignManager.ActivePoliciesContainsValidMap());
            //        if (Find.Maps.Count == 1 && !AssignManager.ActivePoliciesContainsValidMap())
            //        {
            //            Log.Message("ENTER 1");
            //            this means the player was on the move without any base
            //            and just re - settled using a caravan. So, let's move the settings to
            //            the new map
            //            int newMapId = Find.CurrentMap.uniqueID;
            //            AssignManager.MoveLinksToMap(map.mapId, newMapId);
            //            map.mapId = newMapId;
            //        }
            //        if (Find.Maps.Count == 0 && !AssignManager.ActivePoliciesContainsValidMap())
            //        {
            //            Log.Message("ENTER 0");
            //            this means the player is on a Grav ship and has liftoff.So, let's move the 
            //             settings to the new map
            //            int mapid = Find.CurrentMap.u
            //            AssignManager.MoveLinksToMap(mapid);
            //            map.mapId = mapid;
            //        }
            if (!map.IsPlayerHome) 
            {
                AssignManager.DeleteLinksInMap(map.uniqueID);
                MapActivePolicy mapActivePolicy = AssignManager.GetActiveMap(map.uniqueID);
                AssignManager.DeleteMap(mapActivePolicy);
            }
        }

        internal static void ProcessNewMap(Map newMap)
        {
            ProcessNewMapTransition(newMap);
        }

        internal static void UpdateState(List<AssignLink> links, List<Pawn> pawns, Policy policy)
        {
            List<AssignLink> mapLinks = null;
            List<AssignLink> zoneLinks = null;
            int currentMap = Find.CurrentMap.uniqueID;

            //get all links from the current map
            mapLinks = links.FindAll(x => x.mapId == currentMap);
            //get all links from the selected zone
            zoneLinks = mapLinks.FindAll(x => x.zone == policy.id);

            foreach (Pawn p in pawns)
            {
                if (p == null)
                {
                    continue;
                }

                foreach (AssignLink l in zoneLinks)
                {
                    if (l.colonist != null && PawnCompatibility.SamePawn(l.colonist, p))
                    {
                        if (p.playerSettings != null)
                        {
                            l.hostilityResponse = p.playerSettings.hostilityResponse;
                            l.medicinePolicy = p.playerSettings.medCare;
                        }
                        if (p.foodRestriction != null)
                        {
                            l.foodPolicy = p.foodRestriction.CurrentFoodPolicy;
                        }
                        if (p.outfits != null)
                        {
                            l.outfit = p.outfits.CurrentApparelPolicy;
                        }
                        if (Settings.saveInventoryStock && PawnCompatibility.SupportsInventoryStock(p))
                        {
                            l.SetInventoryStockForMedicine(p.inventoryStock);
                        }
                    }
                }
            }

            AssignManager.SetActivePolicy(policy);
        }

        internal static void LoadState(List<AssignLink> links, List<Pawn> pawns, Policy policy)
        {
            if (policy == null)
            {
                return;
            }
            if (links == null || pawns == null)
            {
                AssignManager.SetActivePolicy(policy);
                return;
            }

            int currentMap = Find.CurrentMap.uniqueID;

            //get all links from the current map
            List<AssignLink> mapLinks = links.FindAll(x => x != null && x.mapId == currentMap);
            //get all links from the selected zone
            List<AssignLink> zoneLinks = mapLinks.FindAll(x => x.zone == policy.id);

            foreach (Pawn p in pawns)
            {
                if (!PawnCompatibility.SupportsAssign(p))
                {
                    continue;
                }

                AssignLink link = zoneLinks.FirstOrDefault(l => l.colonist != null && PawnCompatibility.SamePawn(l.colonist, p));
                if (link != null)
                {
                    ApplyLinkToPawn(p, link);
                }
            }

            AssignManager.SetActivePolicy(policy);
        }

        private static void ApplyLinkToPawn(Pawn pawn, AssignLink link)
        {
            ApparelPolicy previousOutfit = pawn.outfits?.CurrentApparelPolicy;
            ApparelPolicy targetOutfit = null;

            if (PawnCompatibility.SupportsApparel(pawn))
            {
                targetOutfit = ResolveOutfit(link.outfit, previousOutfit);
                if (targetOutfit != null)
                {
                    pawn.outfits.CurrentApparelPolicy = targetOutfit;
                }
            }
            if (PawnCompatibility.SupportsDrugs(pawn))
            {
                DrugPolicy drugPolicy = ResolveDrugPolicy(link.drugPolicy, pawn.drugs.CurrentPolicy);
                if (drugPolicy != null)
                {
                    pawn.drugs.CurrentPolicy = drugPolicy;
                }
            }
            if (PawnCompatibility.SupportsFood(pawn))
            {
                FoodPolicy foodPolicy = ResolveFoodPolicy(link.foodPolicy, pawn.foodRestriction.CurrentFoodPolicy);
                if (foodPolicy != null)
                {
                    pawn.foodRestriction.CurrentFoodPolicy = foodPolicy;
                }
            }
            if (PawnCompatibility.SupportsReading(pawn))
            {
                ReadingPolicy readingPolicy = ResolveReadingPolicy(link.readingPolicy, pawn.reading.CurrentPolicy);
                if (readingPolicy != null)
                {
                    pawn.reading.CurrentPolicy = readingPolicy;
                }
            }
            if (PawnCompatibility.SupportsPlayerSettings(pawn))
            {
                pawn.playerSettings.hostilityResponse = link.hostilityResponse;
                pawn.playerSettings.medCare = link.medicinePolicy;
            }

            if (Settings.saveInventoryStock && PawnCompatibility.SupportsInventoryStock(pawn))
            {
                pawn.SetInventoryStock(InventoryStockGroupDefOf.Medicine, link.carriedMedicineThing, link.carriedMedicineCount);
            }

            if (PawnCompatibility.SupportsWeapons(pawn) && Widget_CombatExtended.CombatExtendedAvailable)
            {
                Widget_CombatExtended.SetLoadoutById(pawn, link.loadoutId);
            }
            if (PawnCompatibility.SupportsWeapons(pawn) && Widget_CompositableLoadouts.CompositableLoadoutsAvailable)
            {
                Widget_CompositableLoadouts.SetLoadoutById(pawn, link.compositableState);
            }

            if (targetOutfit != null)
            {
                OutfitStandsPlusCompatibility.TryUseAssignedStandForPolicy(pawn, previousOutfit, targetOutfit);
            }
        }

        private static ApparelPolicy ResolveOutfit(ApparelPolicy savedPolicy, ApparelPolicy currentPolicy)
        {
            if (OutfitExits(savedPolicy))
            {
                return savedPolicy;
            }
            if (OutfitExits(currentPolicy))
            {
                return currentPolicy;
            }
            return OutfitExits(DefaultOutfit) ? DefaultOutfit : Current.Game.outfitDatabase.DefaultOutfit();
        }

        private static DrugPolicy ResolveDrugPolicy(DrugPolicy savedPolicy, DrugPolicy currentPolicy)
        {
            if (DrugPolicyExits(savedPolicy))
            {
                return savedPolicy;
            }
            if (DrugPolicyExits(currentPolicy))
            {
                return currentPolicy;
            }
            return DrugPolicyExits(DefaultDrugPolicy) ? DefaultDrugPolicy : Current.Game.drugPolicyDatabase.DefaultDrugPolicy();
        }

        private static FoodPolicy ResolveFoodPolicy(FoodPolicy savedPolicy, FoodPolicy currentPolicy)
        {
            if (FoodPolicyExists(savedPolicy))
            {
                return savedPolicy;
            }
            if (FoodPolicyExists(currentPolicy))
            {
                return currentPolicy;
            }
            return FoodPolicyExists(DefaultFoodPolicy) ? DefaultFoodPolicy : Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
        }

        private static ReadingPolicy ResolveReadingPolicy(ReadingPolicy savedPolicy, ReadingPolicy currentPolicy)
        {
            if (ReadingPolicyExits(savedPolicy))
            {
                return savedPolicy;
            }
            if (ReadingPolicyExits(currentPolicy))
            {
                return currentPolicy;
            }
            return ReadingPolicyExits(DefaultReadingPolicy) ? DefaultReadingPolicy : Current.Game.readingPolicyDatabase.DefaultReadingPolicy();
        }

        internal static void LoadState(Policy policy)
        {
            List<Pawn> pawns = AssignManager.Colonists().Where(PawnCompatibility.SupportsAssign).ToList();
            LoadState(AssignManager.links, pawns, policy);
        }

        internal static bool OutfitExits(ApparelPolicy outfit)
        {
            if (outfit == null || Current.Game?.outfitDatabase == null)
            {
                return false;
            }

            foreach (ApparelPolicy current in Current.Game.outfitDatabase.AllOutfits)
            {
                if (current.Equals(outfit))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool DrugPolicyExits(DrugPolicy drugPolicy)
        {
            if (drugPolicy == null || Current.Game?.drugPolicyDatabase == null)
            {
                return false;
            }

            foreach (DrugPolicy drug in Current.Game.drugPolicyDatabase.AllPolicies)
            {
                if (drug.Equals(drugPolicy))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool ReadingPolicyExits(ReadingPolicy readingPolicy)
        {
            if (readingPolicy == null || Current.Game?.readingPolicyDatabase == null)
            {
                return false;
            }

            foreach (ReadingPolicy reading in Current.Game.readingPolicyDatabase.AllReadingPolicies)
            {
                if (reading.Equals(readingPolicy))
                {
                    return true;
                }
            }
            return false;
        }


        internal static void CopyToClipboard()
        {
            //Save state in case user has made changes to the active policy
            AssignManager.SaveCurrentState(AssignManager.Colonists().Where(PawnCompatibility.SupportsAssign).ToList());

            Policy policy = GetActivePolicy();
            //if (AssignManager.clipboard != null)
            //{
            //    clipboard = new List<AssignLink>();
            //}

            AssignManager.clipboard.Clear();
            foreach (AssignLink link in AssignManager.links)
            {
                if (link.zone == policy.id)
                {
                    AssignManager.clipboard.Add(new AssignLink(link));
                }
            }
        }

        internal static void PasteToActivePolicy()
        {
            Policy policy = GetActivePolicy();
            if (!AssignManager.clipboard.NullOrEmpty() && AssignManager.clipboard[0].zone != policy.id)
            {
                AssignManager.links.RemoveAll(x => x.zone == policy.id);
                foreach (AssignLink copiedLink in AssignManager.clipboard)
                {
                    copiedLink.zone = policy.id;
                    AssignManager.links.Add(copiedLink);
                }
                AssignManager.LoadState(links, AssignManager.Colonists().Where(PawnCompatibility.SupportsAssign).ToList(), policy);
            }
        }

        internal static void SetDefaultsForFreeColonist(Pawn p)
        {
            if (p != null)
            {
                if (PawnCompatibility.SupportsApparel(p))
                {
                    p.outfits.CurrentApparelPolicy = AssignManager.DefaultOutfit;
                }
                if (PawnCompatibility.SupportsDrugs(p))
                {
                    p.drugs.CurrentPolicy = AssignManager.DefaultDrugPolicy;
                }
                if (PawnCompatibility.SupportsFood(p))
                {
                    p.foodRestriction.CurrentFoodPolicy = AssignManager.DefaultFoodPolicy;
                }
            }
        }

        internal static void SetDefaultsForPrisoner(Pawn p)
        {
            if (PawnCompatibility.SupportsFood(p))
            {
                p.foodRestriction.CurrentFoodPolicy = AssignManager.DefaultPrisonerFoodPolicy;
            }
        }

        internal static void SetDefaultsForSlave(Pawn p)
        {
            if (p != null)
            {
                if (PawnCompatibility.SupportsApparel(p))
                {
                    p.outfits.CurrentApparelPolicy = AssignManager.DefaultSlaveOutfit;
                }
                if (PawnCompatibility.SupportsDrugs(p))
                {
                    p.drugs.CurrentPolicy = AssignManager.DefaultSlaveDrugPolicy;
                }
                if (PawnCompatibility.SupportsFood(p))
                {
                    p.foodRestriction.CurrentFoodPolicy = AssignManager.DefaultSlaveFoodPolicy;
                }
            }
        }

        internal static void PrintAllAssignPolicies(string spacer = "\n")
        {
            Log.Message("[BPC] === List Policies START [" + AssignManager.policies.Count + "] ===");
            foreach (Policy p in AssignManager.policies)
            {
                Log.Message("[BPC]\t" + p.ToString());
            }

            Log.Message("[BPC] === List ActivePolices START [" + AssignManager.activePolicies.Count + "] ===");
            foreach (MapActivePolicy m in AssignManager.activePolicies)
            {
                Log.Message("[BPC]\t" + m.ToString());
            }

            Log.Message("[BPC] === List links START [" + AssignManager.links.Count + "] ===");
            foreach (AssignLink assignLink in AssignManager.links)
            {
                Log.Message("[BPC]\t" + assignLink.ToString());
            }

            Log.Message(spacer);
        }


    }
}


