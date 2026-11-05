using StaadPro.Interop.Core.Common;

namespace StaadPro.Interop.Core.Interfaces
{
    /// <summary>Identifies a managed load combination and its title.</summary>
    public interface ILoadCombination : IEntity
    {
        /// <summary>Gets the combination title.</summary>
        string Title { get; }
    }
}
