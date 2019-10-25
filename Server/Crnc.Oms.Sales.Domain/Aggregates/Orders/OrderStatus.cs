namespace Crnc.Oms.Sales.Domain.Aggregates.Orders
{
    /// <summary>
    /// Status of order
    /// </summary>
    public enum OrderStatus
    {
        NotSent=1,

        NeedSignoff=2,

        Signed=3,

        ConvertedToJob=4,

        Closed=5   
    }
}
