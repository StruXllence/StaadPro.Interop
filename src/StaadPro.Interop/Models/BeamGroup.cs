using System.Collections.Generic;
using StaadPro.Interop.Entities;

namespace StaadPro.Interop.Models
{
    /// <summary>Stores an editable grouping key and an owned list of original analytical beam references.</summary>
    /// <typeparam name="T">The grouping key type; plan-angle grouping uses Double degrees.</typeparam>
    /// <remarks>Each new group starts with an independent empty list. Setters retain supplied values/references without
    /// validation. Edits affect detached metadata only and do not regroup geometry or update STAAD. Query-produced
    /// groups own their list storage but retain original Beam objects, whose later edits remain visible.</remarks>
    public class BeamGroup<T>
    {
        /// <summary>Creates a default key and a fresh empty beam list.</summary>
        public BeamGroup() { }
        /// <summary>Gets or sets the stored grouping key.</summary>
        /// <value>The supplied value. Plan-angle results retain their first qualifying beam's start bearing as the seed.</value>
        public T Key { get; set; }
        /// <summary>Gets or sets the original beams represented by this group.</summary>
        /// <value>An independently initialized editable list; replacing it stores the supplied reference, including null.</value>
        public List<Beam> Beams { get; set; } = new List<Beam>();
    }
}
