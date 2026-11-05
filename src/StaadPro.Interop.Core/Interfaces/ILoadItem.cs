using StaadPro.Interop.Core.Entities;
using StaadPro.Interop.Core.Enums;

namespace StaadPro.Interop.Core.Interfaces
{
    /// <summary>
    /// Contract defining a structural load item associated with a load case.
    /// </summary>
    public interface ILoadItem
    {
        /// <include file="../Documentation/ApiMembers.xml" path="doc/members/member[@name='ILoadItem.P:LoadCase']/*" />
        ILoadCase LoadCase { get; set; }
        /// <include file="../Documentation/ApiMembers.xml" path="doc/members/member[@name='ILoadItem.P:LoadItemType']/*" />
        LoadItemType LoadItemType { get; }
    }
}
