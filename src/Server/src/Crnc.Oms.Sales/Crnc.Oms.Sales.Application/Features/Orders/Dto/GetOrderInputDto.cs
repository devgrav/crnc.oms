using System;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto
{
    public class GetOrderInputDto
        : IUseCaseQuery<GetOrderOutputDto>
    {
        public Guid Id { get; set; }
    }
}