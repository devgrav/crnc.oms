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
    public class GetOrdersForTable
        : IUseCaseQueryHandler<GetOrdersForTableInputDto, GetOrdersForTableOutputDto>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersForTable(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<GetOrdersForTableOutputDto> HandleAsync(GetOrdersForTableInputDto queryData, CancellationToken cancellationToken = default)
        {
            var items = (await _orderRepository.FindAllAsync(cancellationToken))
                .Select(x => new GetOrdersForTableItemOutputDto()
                {
                    
                    Id = x.Id,
                    Number = x.Number,
                    Customer = x.Customer.FullName.Value,
                    CreatedDate = x.DateCreated.ToStandartFormatWithTime(),
                    JobType = EnumHelper.GetDescription(x.JobType),
                    JobDescription = x.JobDescription,
                    JobTypeEnum = x.JobType,
                    CustomerSignOffType = EnumHelper.GetDescription(x.SignOffType),
                    DateSentToCustomer = x.DateSentToCustomer.ToStandartFormatWithTime(),
                    CustomerSignOffTypeEnum = x.SignOffType,
                    Status = EnumHelper.GetDescription(x.Status)
                })
                .ToList();

            return new GetOrdersForTableOutputDto()
            {
                Items = items
            };
        }
    }
}