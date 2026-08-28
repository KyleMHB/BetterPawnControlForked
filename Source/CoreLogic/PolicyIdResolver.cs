using System.Collections.Generic;

namespace BetterPawnControlForked.CoreLogic
{
    internal static class PolicyIdResolver
    {
        internal static int Resolve(int requestedPolicyId, IEnumerable<int> availablePolicyIds)
        {
            if (availablePolicyIds != null)
            {
                foreach (var policyId in availablePolicyIds)
                {
                    if (policyId == requestedPolicyId)
                    {
                        return requestedPolicyId;
                    }
                }
            }

            return 0;
        }
    }
}
