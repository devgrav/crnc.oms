using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Microsoft.EntityFrameworkCore;
using Crnc.Oms.Sales.Application.Helpers;
using Crnc.Oms.Sales.Domain.Repositories;

namespace Crnc.Oms.Sales.Application.Features.Orders.Queries
{
    public class GetOrdersForTableHandler
        : IUseCaseQueryHandler<GetOrdersForTableInputDto, GetOrdersForTableOutputDto>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersForTableHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }


        public async Task<GetOrdersForTableOutputDto> Handle(GetOrdersForTableInputDto request, CancellationToken cancellationToken)
        {
            var items = (await _orderRepository.FindAllAsync(cancellationToken))
                .Select(x => new GetOrdersForTableItemOutputDto()
                {
                    
                    Id = x.Id,
                    Number = x.Number,
                    CreatedDate = x.DateCreated.ToStandartFormatWithTime(),
                    Customer = x.Customer.Title.Value,
                    JobType = EnumHelper.GetDescription(x.JobType),
                    JobDescription = x.JobDescription,
                    DateSentToCustomer = x.DateSentToCustomer.ToStandartFormatWithTime(),
                    CustomerSignOffType = EnumHelper.GetDescription(x.SignOffType),
                    Status = x.Status.DisplayName
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return new GetOrdersForTableOutputDto()
            {
                Items = items
            };
        }
    }
}