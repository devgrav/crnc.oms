using System.ComponentModel;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    /// <summary>
    /// Type of signoff for estimate
    /// </summary>
    public enum SignoffType
    {
        [Description("Email")]
        Email=1,
        
        [Description("Verbal")]
        Verbal=2
    }
}
