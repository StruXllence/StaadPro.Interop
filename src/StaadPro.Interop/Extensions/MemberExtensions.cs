using System;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Services.Geometry.Detached;

namespace StaadPro.Interop.Extensions
{
    /// <summary>Provides detached connectivity observations for supplied physical members.</summary>
    public static class MemberExtensions
    {
        /// <summary>Determines whether two physical members share an equivalent node within the selected scope.</summary>
        /// <param name="member">Non-null first member. The selected endpoints or Nodes collection must contain finite initialized nodes.</param>
        /// <param name="targetMember">Non-null comparison member qualified using the same scope as the first member.</param>
        /// <param name="scope">EndpointsOnly (0, the default) selects StartNode and EndNode on both members.
        /// AnyNode (1) selects each member's supplied Nodes collection, including any supplied interior nodes.
        /// Other values throw before geometry is read.</param>
        /// <returns>True when at least one selected pair has matching IDs and three-dimensional Euclidean separation
        /// at most Constants.Tolerance (0.001 model length units). Returns false when no pair matches, including when
        /// either valid AnyNode collection is empty. A valid nonempty member connects to itself.</returns>
        /// <remarks><para>EndpointsOnly requires both endpoints on both members and ignores Nodes/Beams collections.
        /// AnyNode uses exactly the supplied Nodes collections, without adding endpoints, scanning Beams or hydrating
        /// missing connectivity. Endpoints outside those collections do not participate. This preserves the selected-node
        /// semantics and supports an interior T-junction only when its node is supplied in the participating collections.</para>
        /// <para>Captures each selected ID/coordinate once while enumerating each selected collection once, then checks
        /// the fully qualified snapshots. All selected nodes are validated before matching, even after an apparent early
        /// match or when the other collection is empty. Missing collections, null selected nodes and nonfinite coordinates
        /// throw rather than produce a partial result. Invalid unused geometry is outside the selected scope.</para>
        /// <para>Uses an ID index and the existing scaled Euclidean equality rule. Matching IDs still require the distance
        /// test; coordinate coincidence with another ID does not connect members. Repeated IDs with differing coordinates
        /// are compared individually. Captured values avoid dependence on mutable HashSet hashing and cached connected-member
        /// lists. An ID bucket may require pairwise comparisons when many selected nodes share the same ID.</para>
        /// <para>Both members must use compatible model context, coordinate system and length units. Draft IDs are allowed;
        /// native membership and solver connectivity cannot be inferred. No STAAD acquisition, selection, geometry hydration,
        /// unit conversion, merge, save, analysis or mutation occurs. Concurrent writers must be coordinated by the caller;
        /// snapshots are not atomic. Enumeration failures propagate and subsequent edits do not recompute the Boolean.</para></remarks>
        /// <exception cref="ArgumentNullException">member or targetMember is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">scope is not EndpointsOnly or AnyNode.</exception>
        /// <exception cref="ArgumentException">Selected incidence, a selected Nodes collection or a selected node is missing,
        /// or a selected coordinate is NaN or infinite.</exception>
        /// <exception cref="InvalidOperationException">A selected HashSet is modified during enumeration.</exception>
        /// <example><code>
        /// var joint = new StaadPro.Interop.Entities.Node(2, 1, 0, 0);
        /// var first = new StaadPro.Interop.Entities.Member(1, new StaadPro.Interop.Entities.Node(1, 0, 0, 0), joint);
        /// var second = new StaadPro.Interop.Entities.Member(2, new StaadPro.Interop.Entities.Node(2, 1, 0, 0),
        ///     new StaadPro.Interop.Entities.Node(3, 2, 0, 0));
        /// System.Console.WriteLine(StaadPro.Interop.Extensions.MemberExtensions.IsConnectedToMember(first, second));
        /// System.Console.WriteLine(StaadPro.Interop.Extensions.MemberExtensions.IsConnectedToMember(first, second,
        ///     StaadPro.Interop.Enums.MemberConnectivityScope.AnyNode));
        /// </code></example>
        public static bool IsConnectedToMember(this Member member, Member targetMember,
            MemberConnectivityScope scope = MemberConnectivityScope.EndpointsOnly)
        {
            if (ReferenceEquals(member, null)) throw new ArgumentNullException(nameof(member));
            if (ReferenceEquals(targetMember, null)) throw new ArgumentNullException(nameof(targetMember));
            if (scope != MemberConnectivityScope.EndpointsOnly && scope != MemberConnectivityScope.AnyNode)
                throw new ArgumentOutOfRangeException(nameof(scope));
            var left = GeometryConnectivityService.MemberNodes(member, scope, nameof(member));
            var right = GeometryConnectivityService.MemberNodes(targetMember, scope, nameof(targetMember));
            return GeometryConnectivityService.ShareNode(left, right);
        }
    }
}
