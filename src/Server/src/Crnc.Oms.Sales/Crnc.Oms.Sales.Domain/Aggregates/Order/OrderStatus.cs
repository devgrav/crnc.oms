using System.ComponentModel;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    /// <summary>
    /// Status of order
    /// </summary>
    public enum OrderStatus
    {
        [Description("Not sent")]
        NotSent=1,

        [Description("Need signoff")]
        NeedSignoff=2,

        [Description("Signed")]
        Signed=3,

        [Description("Converted to job")]
        ConvertedToJob=4,

        [Description("Closed")]
        Closed=5   
    }
}
