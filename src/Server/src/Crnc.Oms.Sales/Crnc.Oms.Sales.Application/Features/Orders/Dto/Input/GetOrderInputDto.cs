using System;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Input
{
    public class GetOrderInputDto
        : IUseCaseQuery<GetOrderOutputDto>
    {
        public Guid Id { get; set; }
    }
}