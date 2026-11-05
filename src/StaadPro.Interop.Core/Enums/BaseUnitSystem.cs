using System.ComponentModel;

namespace StaadPro.Interop.Core.Enums
{
    public enum BaseUnitSystem
    {
        [Description("Unknown")]
        Unknown = -1,
        [Description("English")]
        Imperial = 1,
        [Description("Metric")]
        Metric = 2
    }
}
