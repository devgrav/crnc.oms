using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Application.Helpers;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;

namespace Crnc.Oms.Sales.Application.Features.Orders.Queries
{
    public class GetNewOrder
        : IUseCaseQueryHandler<GetNewOrderInputDto,GetNewOrderOutputDto>
    {
        private readonly ICurrentDateTimeProvider _currentDateTimeProvider;

        public GetNewOrder(ICurrentDateTimeProvider currentDateTimeProvider)
        {
            _currentDateTimeProvider = currentDateTimeProvider;
        }
        
        public async Task<GetNewOrderOutputDto> HandleAsync(GetNewOrderInputDto queryData, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new GetNewOrderOutputDto()
            {
                DateCreated = _currentDateTimeProvider.GetNow().ToShortDateString(),
                StatusEnum = OrderStatus.NotSent,
                Customer = new GetNewOrderCustomerOutputDto()
                {
                    Abbreviation = "",
                    Email = "",
                    Phone = "",
                    FullName = ""
                },
                Status = EnumHelper.GetDescription(OrderStatus.NotSent),
                JobDescription = "",
                JobType = ""
            });
        }
    }
}