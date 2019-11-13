using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Crnc.Oms.Sales.Application.Helpers;

namespace Crnc.Oms.Sales.Application.Features.Orders.Queries
{
    public class GetOrdersForTable
        : IUseCaseQueryHandler<OrdersForTableInputDto, OrdersForTableOutputDto>
    {
        private readonly SalesDataContext _context;

        public GetOrdersForTable(SalesDataContext context)
        {
            _context = context;
        }

        public async Task<OrdersForTableOutputDto> HandleAsync(OrdersForTableInputDto queryData, CancellationToken cancellationToken = default)
        {
            var allOrders = await _context.Orders
                .Include(x => x.Customer)
                .ToListAsync(cancellationToken);

            var items = allOrders
                .Select(x => new OrdersForTableItemOutputDto()
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

            return new OrdersForTableOutputDto()
            {
                Items = items
            };
        }
    }
}