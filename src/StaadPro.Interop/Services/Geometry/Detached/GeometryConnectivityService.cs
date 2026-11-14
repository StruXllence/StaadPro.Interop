using System;
using System.Collections.Generic;
using StaadPro.Interop.Entities;
using StaadPro.Interop.Enums;
using StaadPro.Interop.Helpers;

namespace StaadPro.Interop.Services.Geometry.Detached
{
    // Detached equality snapshots; no dependence on mutable Node hashing or cached connectivity.
    internal static class GeometryConnectivityService
    {
        internal sealed class NodeSnapshot
        {
            internal readonly int Id;
            internal readonly double X, Y, Z;
            internal NodeSnapshot(Node node, string parameter)
            {
                if (ReferenceEquals(node, null)) throw new ArgumentException("A participating node is missing.", parameter);
                Id = node.Id; X = node.X; Y = node.Y; Z = node.Z;
                if (!Finite(X) || !Finite(Y) || !Finite(Z))
                    throw new ArgumentException("Participating coordinates must be finite.", parameter);
            }
        }

        internal static NodeSnapshot[] BeamNodes(Beam beam, string parameter)
        {
            Node start = beam.StartNode, end = beam.EndNode;
            return new[] { new NodeSnapshot(start, parameter), new NodeSnapshot(end, parameter) };
        }

        internal static NodeSnapshot[] MemberNodes(Member member, MemberConnectivityScope scope, string parameter)
        {
            if (scope == MemberConnectivityScope.EndpointsOnly)
            {
                Node start = member.StartNode, end = member.EndNode;
                return new[] { new NodeSnapshot(start, parameter), new NodeSnapshot(end, parameter) };
            }
            HashSet<Node> nodes = member.Nodes;
            if (nodes == null) throw new ArgumentException("The selected member Nodes collection is missing.", parameter);
            var snapshots = new List<NodeSnapshot>();
            foreach (Node node in nodes) snapshots.Add(new NodeSnapshot(node, parameter));
            return snapshots.ToArray();
        }

        internal static bool ShareNode(NodeSnapshot[] left, NodeSnapshot[] right)
        {
            // Both sides have already been fully qualified, including malformed nodes after early matches.
            var byId = new Dictionary<int, List<NodeSnapshot>>();
            foreach (NodeSnapshot node in right)
            {
                if (!byId.TryGetValue(node.Id, out List<NodeSnapshot> bucket))
                    byId.Add(node.Id, bucket = new List<NodeSnapshot>());
                bucket.Add(node);
            }
            foreach (NodeSnapshot node in left)
                if (byId.TryGetValue(node.Id, out List<NodeSnapshot> bucket))
                    foreach (NodeSnapshot candidate in bucket)
                        if (Equal(node, candidate)) return true;
            return false;
        }

        private static bool Equal(NodeSnapshot a, NodeSnapshot b)
        {
            double x = Math.Abs(a.X - b.X), y = Math.Abs(a.Y - b.Y), z = Math.Abs(a.Z - b.Z);
            double scale = Math.Max(x, Math.Max(y, z));
            if (scale > Constants.Tolerance) return false; // includes overflowing subtraction
            if (scale == 0) return true;
            x /= scale; y /= scale; z /= scale;
            return scale * Math.Sqrt(x * x + y * y + z * z) <= Constants.Tolerance;
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
