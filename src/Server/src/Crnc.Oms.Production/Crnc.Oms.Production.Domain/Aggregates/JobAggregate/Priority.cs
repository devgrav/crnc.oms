using System.ComponentModel;

namespace Crnc.Oms.Production.Domain.Aggregates.JobAggregate
{
    public enum Priority
    {
        [Description("High")]
        High = 1,
        [Description("Middle")]
        Middle = 2,
        [Description("Low")]
        Low = 3
    }
}