using System.Collections.Generic;
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
            var allJobTypes = new List<TextValueOutputDto<int, string>>()
                {
                    new TextValueOutputDto<int, string>()
                    {
                        Value = 0,
                        Text = "Not chosen"
                    }
                };
            
            var jobTypes =
                EnumHelper.ToDictionaryWithKeysAndDescriptions(JobType.New).Select(x =>
                    new TextValueOutputDto<int, string>()
                    {
                        Text = x.Value,
                        Value = x.Key
                    })
                    .ToList();
            
            allJobTypes.AddRange(jobTypes);
            
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
                JobType = 0,
                JobTypes = jobTypes 
            });
        }
    }
}