using StaadPro.Interop.Entities;

namespace StaadPro.Interop.Models.Geometry
{
    /// <summary>Pairs an original node reference with one captured, unwrapped outward XZ angle in radians.</summary>
    /// <remarks>Immutable result fields replace positional tuple access. The node remains caller-owned and mutable;
    /// later coordinate edits do not recompute AngleInRadians. No native resource or geometric path is owned.
    /// Angles can be negative or exceed pi and the returned sequence is not necessarily sorted after unwrapping.</remarks>
    public sealed class UnwrappedNodeAngle
    {
        internal UnwrappedNodeAngle(Node node, double angleInRadians) { Node = node; AngleInRadians = angleInRadians; }

        /// <summary>Gets the original node associated with this captured angle.</summary>
        /// <value>The non-null caller-owned node reference, not a cloned or hydrated geometry object.</value>
        public Node Node { get; }

        /// <summary>Gets the captured and unwrapped plan angle in radians.</summary>
        /// <value>A finite angle based on atan2(node.Z - origin.Z, node.X - origin.X), then suffix adjustment
        /// by full turns. This is not degrees, an inward bearing, a member local-axis angle or a live observation.</value>
        public double AngleInRadians { get; }
    }
}
