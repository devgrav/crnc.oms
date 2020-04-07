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
    public class GetOrderHandler
        : IUseCaseQueryHandler<GetOrderInputDto,GetOrderOutputDto>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        
  
        public async Task<GetOrderOutputDto> Handle(GetOrderInputDto request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.FindByIdAsync(request.Id,cancellationToken);
            
            if(order == null)
                throw new MissingEntityException("Order not found");

            return new GetOrderOutputDto()
            {
                Id = order.Id,
                CustomerAbbreviation = order.Customer.Title.NameAbbreviation.Value,
                CustomerTitle = order.Customer.Title.Value,
                CustomerContactPersonFirstName = order.Customer.ContactPerson.FullName.FirstName,
                CustomerContactPersonLastName = order.Customer.ContactPerson.FullName.LastName,
                CustomerContactPersonEmail = order.Customer.ContactPerson.Email.Value,
                CustomerContactPersonPhone = order.Customer.ContactPerson.Phone.Value,
                DateCreated = order.DateCreated.ToString("dd.MM.yyyy HH:mm"),
                DateSentToCustomer = order.DateSentToCustomer.HasValue ? order.DateSentToCustomer.Value.ToString("dd.MM.yyyy HH:mm") : "",
                JobDescription = order.JobInfo.Description,
                JobType = order.JobInfo.Type,
                JobId = order.JobInfo.Id,
                JobNumber = order.JobInfo.Number,
                JobTypes = EnumHelper.ToDictionaryWithKeysAndDescriptions(JobType.New).Select(x => new TextValueOutputDto<int, string>()
                {
                    Text = x.Value,
                    Value = x.Key
                }).ToList(),
                Status = (OrderStatusEnum)order.Status.Value,
                Statuses = order.GetAvailableStatusesForCurrent().Select(x => new TextValueOutputDto<int, string>()
                {
                    Text = x.DisplayName,
                    Value = x.Value
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