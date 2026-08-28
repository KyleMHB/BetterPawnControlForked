using Verse;

namespace BetterPawnControlForked
{
    [StaticConstructorOnStartup]
    internal static class LastMapManager
    {
        static internal int lastMapId { get => DataStorage.State.lastMapId; set => DataStorage.State.lastMapId = value; }

        internal static void ForceInit()
        {
            lastMapId = -1;
        }

        //internal static void SetLastMapId(int id)
        //{
        //    lastMapId = id;
        //}
    }
}


