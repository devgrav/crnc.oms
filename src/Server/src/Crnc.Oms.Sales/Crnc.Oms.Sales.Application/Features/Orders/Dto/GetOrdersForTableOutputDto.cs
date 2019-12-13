using System;
using System.Collections.Generic;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto
{
    public class GetOrdersForTableOutputDto
    {
        public List<GetOrdersForTableItemOutputDto> Items { get; set; }
    }
}