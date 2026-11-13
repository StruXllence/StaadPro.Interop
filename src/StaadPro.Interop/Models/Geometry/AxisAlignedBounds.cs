namespace StaadPro.Interop.Models.Geometry
{
    /// <summary>Contains the finite minimum and maximum coordinates of a detached geometry collection.</summary>
    /// <remarks>An immutable axis-aligned snapshot in the supplied entities' coordinate system and length units.
    /// Owns no entities or native resources. It describes the supplied points, not an oriented box,
    /// clipped shape, live model boundary or certified engineering envelope. Later edits do not update it.</remarks>
    public sealed class AxisAlignedBounds
    {
        internal AxisAlignedBounds(double xMin, double xMax, double yMin, double yMax, double zMin, double zMax)
        {
            XMin = xMin; XMax = xMax; YMin = yMin; YMax = yMax; ZMin = zMin; ZMax = zMax;
        }
        /// <summary>Gets the smallest captured X coordinate.</summary>
        /// <value>A finite coordinate in the supplied model length units.</value>
        public double XMin { get; }
        /// <summary>Gets the largest captured X coordinate.</summary>
        /// <value>A finite coordinate greater than or equal to XMin.</value>
        public double XMax { get; }
        /// <summary>Gets the smallest captured Y coordinate.</summary>
        /// <value>A finite coordinate in the supplied model length units.</value>
        public double YMin { get; }
        /// <summary>Gets the largest captured Y coordinate.</summary>
        /// <value>A finite coordinate greater than or equal to YMin.</value>
        public double YMax { get; }
        /// <summary>Gets the smallest captured Z coordinate.</summary>
        /// <value>A finite coordinate in the supplied model length units.</value>
        public double ZMin { get; }
        /// <summary>Gets the largest captured Z coordinate.</summary>
        /// <value>A finite coordinate greater than or equal to ZMin.</value>
        public double ZMax { get; }
    }
}
