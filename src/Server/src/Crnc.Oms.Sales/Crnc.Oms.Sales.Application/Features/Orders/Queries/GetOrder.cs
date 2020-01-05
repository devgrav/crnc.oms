using System.Linq;
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
                CustomerAbbreviation = order.Customer.Title.NameAbbreviation.Value,
                CustomerTitle = order.Customer.Title.Value,
                CustomerContactPersonFirstName = order.Customer.ContactPerson.FullName.FirstName,
                CustomerContactPersonMiddleName = order.Customer.ContactPerson.FullName.MiddleName,
                CustomerContactPersonLastName = order.Customer.ContactPerson.FullName.LastName,
                CustomerContactPersonEmail = order.Customer.ContactPerson.Email.Value,
                CustomerContactPersonPhone = order.Customer.ContactPerson.Phone.Value,
                DateCreated = order.DateCreated.ToString(),
                JobDescription = order.JobDescription,
                JobType = order.JobType,
                JobTypes = EnumHelper.ToDictionaryWithKeysAndDescriptions(JobType.New).Select(x => new TextValueOutputDto<int, string>()
                {
                    Text = x.Value,
                    Value = x.Key
                }).ToList(),
                Status = order.Status,
                Statuses = EnumHelper.ToDictionaryWithKeysAndDescriptions(OrderStatus.NotSent).Select(x => new TextValueOutputDto<int, string>()
                {
                    Text = x.Value,
                    Value = x.Key
                }).ToList(),
                MaterialSource = order.MaterialSource,
                MaterialSources = EnumHelper.ToDictionaryWithKeysAndDescriptions(MaterialSource.Stock).Select(x => new TextValueOutputDto<int, string>()
                {
                    Text = x.Value,
                    Value = x.Key
                }).ToList(),
                SignoffType = order.SignOffType,
                SignoffTypes = EnumHelper.ToDictionaryWithKeysAndDescriptions(SignoffType.Email).Select(x => new TextValueOutputDto<int, string>()
                {
                    Text = x.Value,
                    Value = x.Key
                }).ToList(),
            };
        }
    }
}