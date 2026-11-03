using System;
using System.Collections.Generic;

namespace BetterPawnControlForked.CoreLogic
{
    internal static class PolicyDefaults
    {
        // The vanilla database owns the colonist default; BPC keeps a save reference.
        internal static T Read<T>(ref T saved, Func<T> getDefault) where T : class
        {
            saved = getDefault();
            return saved;
        }

        internal static void Set<T>(ref T saved, T value, ICollection<T> policies, Action<T> setDefault) where T : class
        {
            if (value == null || !policies.Contains(value))
            {
                return;
            }
            setDefault(value);
            saved = value;
        }

        internal static T ReadSeparate<T>(ref T saved, ICollection<T> policies, Func<T> getDefault) where T : class
        {
            if (saved == null || !policies.Contains(saved))
            {
                saved = getDefault();
            }
            return saved;
        }

        internal static void Restore<T>(ref T saved, ICollection<T> policies, Func<T> getDefault, Action<T> setDefault) where T : class
        {
            Set(ref saved, saved, policies, setDefault);
            Read(ref saved, getDefault);
        }
    }
}
