namespace StaadPro.Interop.Enums
{
    /// <summary>Specifies the candidate plate corner criterion used for connectivity grouping.</summary>
    public enum PlateConnectivityScope
    {
        /// <summary>For IsConnectedToPlate, requires a matching consecutive edge in both retained loops, in either direction.</summary>
        SharedEdge = 0,
        /// <summary>Requires at least one common distinct, non-null corner node.</summary>
        AnyCorner = 1
    }
}
