namespace Crnc.Oms.Sales.Domain.Aggregates.Orders
{
    /// <summary>
    /// Source of material
    /// </summary>
    public enum MaterialSource
    {
        ToBeOrdered =1,

        IncludedByCustomer = 2,

        UserStock = 3
    }
}
