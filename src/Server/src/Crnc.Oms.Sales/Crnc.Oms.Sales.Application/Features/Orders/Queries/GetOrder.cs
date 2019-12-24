using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Exceptions;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Application.Helpers;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;

namespace Crnc.Oms.Sales.Application.Features.Orders.Queries
{
    public class GetOrder
        : IUseCaseQueryHandler<GetOrderInputDto,GetOrderOutputDto>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrder(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
        public async Task<GetOrderOutputDto> HandleAsync(GetOrderInputDto queryData, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.FindByIdAsync(queryData.Id,cancellationToken);
            
            if(order == null)
                throw new MissingEntityException("Order not found");

            return new GetOrderOutputDto()
            {
                Id = order.Id,
                Status = EnumHelper.GetDescription(order.Status),
                Customer = new GetNewOrderCustomerOutputDto()
                {
                    Abbreviation = order.Customer.Abbreviation.Value,
                    Email = order.Customer.Email.Value,
                    Phone = order.Customer.Phone.Value,
                    FullName = order.Customer.FullName.Value
                },
                DateCreated = order.DateCreated.ToString(),
                JobDescription = order.JobDescription,
                JobType = EnumHelper.GetDescription(order.JobType),
                StatusEnum = order.Status,
                JobTypeEnum = order.JobType
            };
        }
    }
}