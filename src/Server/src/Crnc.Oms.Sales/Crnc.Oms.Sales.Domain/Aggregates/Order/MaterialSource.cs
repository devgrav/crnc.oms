namespace Crnc.Oms.Sales.Domain.Aggregates.Order
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
