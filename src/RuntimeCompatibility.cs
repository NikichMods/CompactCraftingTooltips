// SPDX-License-Identifier: MPL-2.0

using System;

namespace CompactCraftingTooltips
{
    internal enum HostIdentityStatus
    {
        Verified1407,
        Unverified
    }

    internal static class RuntimeCompatibility
    {
        internal static readonly Guid VerifiedAssemblyCSharpMvid =
            new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");

        internal static HostIdentityStatus Classify(Guid? assemblyCSharpMvid)
        {
            return assemblyCSharpMvid.HasValue &&
                   assemblyCSharpMvid.Value ==
                       VerifiedAssemblyCSharpMvid
                ? HostIdentityStatus.Verified1407
                : HostIdentityStatus.Unverified;
        }

        internal static string DescribeHost(Guid? assemblyCSharpMvid)
        {
            if (!assemblyCSharpMvid.HasValue)
                return "unverified; assembly-csharp-mvid=unavailable";

            string status =
                Classify(assemblyCSharpMvid) ==
                HostIdentityStatus.Verified1407
                    ? "verified-gk-1.407"
                    : "unverified";

            return status +
                   "; assembly-csharp-mvid=" +
                   assemblyCSharpMvid.Value.ToString("D");
        }
    }

    internal sealed class SessionCircuitBreaker
    {
        private bool _disabled;
        private bool _failureReported;

        internal bool IsDisabled
        {
            get { return _disabled; }
        }

        internal bool DisableAndShouldReport()
        {
            _disabled = true;

            if (_failureReported)
                return false;

            _failureReported = true;
            return true;
        }
    }
}
