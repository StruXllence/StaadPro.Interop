using StaadPro.Interop.Entities;

namespace StaadPro.Interop.Models.Geometry
{
    /// <summary>Contains one directed pair of consecutive retained plate-corner references.</summary>
    /// <remarks>The endpoint slots are immutable and retain original Node references rather than cloned coordinates.
    /// Later node edits remain visible; later plate-slot edits do not replace these endpoints. This named result does
    /// not calculate length or certify nonzero geometry, native membership or plate validity. A one-corner extracted
    /// loop produces a self-edge; two-corner extraction produces both directions. No native resources are owned.</remarks>
    public sealed class PlateEdge
    {
        internal PlateEdge(Node start, Node end) { Start = start; End = end; }

        /// <summary>Gets the original retained node at the start of this directed edge.</summary>
        /// <value>A non-null Node reference captured from the plate's retained winding order.</value>
        public Node Start { get; }

        /// <summary>Gets the next retained node, wrapping from the last corner to the first.</summary>
        /// <value>A non-null Node reference. It can equal Start for a one-corner extracted loop.</value>
        public Node End { get; }
    }
}
