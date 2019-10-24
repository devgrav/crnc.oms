namespace Crnc.Oms.Sales.Domain.Aggregates.Estimates
{
    /// <summary>
    /// Status of estimate
    /// </summary>
    public enum EstimateStatus
    {
        NotSent=1,

        NeedSignoff=2,

        Signed=3,

        ConvertedToJob=4,

        Closed=5   
    }
}
