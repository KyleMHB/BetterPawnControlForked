using Verse;
using static BetterPawnControlForked.BetterPawnControlForkedMod;

namespace BetterPawnControlForked.Helpers
{
    public static class PawnExtensions
    {
        public static void SetInventoryStock(this Pawn pawn, InventoryStockGroupDef inventoryStockGroupDef, ThingDef thing, int count)
        {
            if (pawn == null || pawn.inventoryStock == null)
                return;

            if (thing == null)
                return;

            pawn.inventoryStock.SetThingForGroup(inventoryStockGroupDef, thing);
            pawn.inventoryStock.SetCountForGroup(inventoryStockGroupDef, count);
        }
    }
}


