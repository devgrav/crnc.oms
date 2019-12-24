using System.Collections.Generic;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Output
{
    public class GetOrdersForTableOutputDto
    {
        public List<GetOrdersForTableItemOutputDto> Items { get; set; }
    }
}