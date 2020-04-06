using System.ComponentModel;

namespace Crnc.Oms.Production.Domain.Aggregates.JobAggregate
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
