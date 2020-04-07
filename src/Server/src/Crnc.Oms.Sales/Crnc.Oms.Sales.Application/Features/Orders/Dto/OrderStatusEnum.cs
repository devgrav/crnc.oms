using System.ComponentModel;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto
{
    public enum OrderStatusEnum
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