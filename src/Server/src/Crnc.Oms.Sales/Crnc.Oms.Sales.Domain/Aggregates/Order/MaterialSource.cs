using System.ComponentModel;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    /// <summary>
    /// Source of material
    /// </summary>
    public enum MaterialSource
    {
        [Description("To be ordered")]
        ToBeOrdered =1,

        [Description("Included by customer")]
        IncludedByCustomer = 2,

        [Description("Stock")]
        Stock = 3
    }
}
