using System;

namespace BetterPawnControlForked.CoreLogic
{
    internal static class PawnIdentity
    {
        internal static bool Same<T>(T left, T right, Func<T, string> keySelector) where T : class
        {
            if (ReferenceEquals(left, right))
            {
                return left != null;
            }

            if (left == null || right == null || keySelector == null)
            {
                return false;
            }

            string leftKey;
            string rightKey;
            try
            {
                leftKey = keySelector(left);
                rightKey = keySelector(right);
            }
            catch (Exception)
            {
                return false;
            }

            return !string.IsNullOrEmpty(leftKey)
                && !string.IsNullOrEmpty(rightKey)
                && string.Equals(leftKey, rightKey, StringComparison.Ordinal);
        }
    }
}
