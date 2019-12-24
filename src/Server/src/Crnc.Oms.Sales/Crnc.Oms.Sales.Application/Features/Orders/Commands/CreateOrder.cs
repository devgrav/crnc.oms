using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Application.Features.Orders.Commands
{
    public class CreateOrder
        : IUseCaseCommandHandler<CreateOrderInputDto, CreateOrderOutputDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentDateTimeProvider _currentDateTimeProvider;

        public CreateOrder(IOrderRepository orderRepository, ICurrentDateTimeProvider currentDateTimeProvider)
        {
            _orderRepository = orderRepository;
            _currentDateTimeProvider = currentDateTimeProvider;
        }
        
        public async Task<CreateOrderOutputDto> HandleAsync(CreateOrderInputDto command, CancellationToken cancellationToken = default)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));
            
            var customer = new Customer(new FullName
                (
                    command.FirstName,
                    command.LastName,
                    command.MiddleName
                ),
                new NameAbbreviation(command.Abbreviation),
                new Email(command.Email),
                new Phone(command.Phone)
            );

            var order = new Order(
                Guid.NewGuid(), 
                _currentDateTimeProvider.GetNow(), 
                command.JobType, 
                command.JobDescription, 
                customer);
            
            _orderRepository.Add(order);

            await _orderRepository.SaveChangesAsync(cancellationToken);

            return new CreateOrderOutputDto()
            {
                Id = order.Id
            };
        }
    }
}