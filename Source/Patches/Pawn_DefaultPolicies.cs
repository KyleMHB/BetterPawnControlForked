using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterPawnControlForked
{
    internal static class PawnLifecycleDefaults
    {
        internal static void Apply(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            AssignManager.LinksCleanUp();
            if (AssignManager.links.Exists(link => PawnCompatibility.SamePawn(pawn, link?.colonist)))
            {
                return;
            }

            if (pawn.IsSlave)
            {
                AssignManager.SetDefaultsForSlave(pawn);
            }
            else if (pawn.IsPrisoner)
            {
                AssignManager.SetDefaultsForPrisoner(pawn);
            }
            else if (pawn.IsFreeColonist
                || (PawnCompatibility.SupportsAssign(pawn) && pawn.Faction == PawnCompatibility.PlayerFaction))
            {
                AssignManager.SetDefaultsForFreeColonist(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
    internal static class Pawn_GuestTracker_SetGuestStatus
    {
        internal static void Postfix(Pawn ___pawn)
        {
            PawnLifecycleDefaults.Apply(___pawn);
        }
    }

    [HarmonyPatch(typeof(Faction), nameof(Faction.Notify_PawnJoined))]
    internal static class Faction_Notify_PawnJoined
    {
        internal static void Postfix(Pawn p)
        {
            PawnLifecycleDefaults.Apply(p);
        }
    }
}
