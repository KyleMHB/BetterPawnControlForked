using System;

namespace BetterPawnControlForked
{
    internal static class PawnTableSession
    {
        internal static Type ActiveWindowType { get; private set; }

        internal static void Open(Type windowType)
        {
            ActiveWindowType = windowType;
        }

        internal static void Close(Type windowType)
        {
            if (ActiveWindowType == windowType)
            {
                ActiveWindowType = null;
            }
        }
    }
}
