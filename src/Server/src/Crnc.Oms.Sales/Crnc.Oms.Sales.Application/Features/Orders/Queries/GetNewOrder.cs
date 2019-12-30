using System.Linq;
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
                CustomerAbbreviation = "",
                CustomerTitle = "",
                CustomerContactPersonFirstName = "",
                CustomerContactPersonMiddleName = "",
                CustomerContactPersonLastName = "",
                CustomerContactPersonEmail = "",
                CustomerContactPersonPhone = "",
                JobDescription = "",
                JobType = JobType.New,
                JobTypes = EnumHelper.ToDictionaryWithKeysAndDescriptions(JobType.New).Select(x => new TextValueOutputDto<int, string>()
                {
                    Text = x.Value,
                    Value = x.Key
                }).ToList()
            });
        }
    }
}