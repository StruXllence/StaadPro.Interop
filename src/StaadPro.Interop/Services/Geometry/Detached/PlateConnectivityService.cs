using System.Collections.Generic;
using StaadPro.Interop.Entities;

namespace StaadPro.Interop.Services.Geometry.Detached
{
    internal static class PlateConnectivityService
    {
        internal sealed class Corner
        {
            internal readonly Node Node;
            internal readonly GeometryConnectivityService.NodeSnapshot Values;
            internal Corner(Node node, string parameter)
            {
                Node = node;
                Values = new GeometryConnectivityService.NodeSnapshot(node, parameter);
            }
        }

        internal static Corner[] CaptureLoop(Plate plate, string parameter)
        {
            var nodes = new[] { plate.A, plate.B, plate.C, plate.D };
            var captured = new Corner[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                if (!ReferenceEquals(nodes[i], null)) captured[i] = new Corner(nodes[i], parameter);

            var retained = new List<Corner>();
            foreach (var corner in captured)
            {
                if (corner == null) continue;
                bool duplicate = false;
                foreach (var representative in retained)
                    if (Equal(representative, corner)) { duplicate = true; break; }
                if (!duplicate) retained.Add(corner);
            }
            return retained.ToArray();
        }

        internal static bool ShareCorner(Corner[] left, Corner[] right)
        {
            foreach (var first in left)
                foreach (var second in right)
                    if (Equal(first, second)) return true;
            return false;
        }

        internal static bool ShareEdge(Corner[] left, Corner[] right)
        {
            // A self-edge from one retained corner must not promote corner-only contact to edge contact.
            if (left.Length < 2 || right.Length < 2) return false;
            for (int i = 0; i < left.Length; i++)
                for (int j = 0; j < right.Length; j++)
                {
                    var a = left[i]; var b = left[(i + 1) % left.Length];
                    var c = right[j]; var d = right[(j + 1) % right.Length];
                    if ((Equal(a, c) && Equal(b, d)) || (Equal(a, d) && Equal(b, c))) return true;
                }
            return false;
        }

        private static bool Equal(Corner first, Corner second) => GeometryConnectivityService.Equal(first.Values, second.Values);
    }
}
