using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Factories;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Application.Features.Orders.Commands
{
    public class CreateOrderHandler
        : IUseCaseCommandHandler<CreateOrderInputDto, CreateOrderOutputDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentDateTimeProvider _currentDateTimeProvider;
        private readonly ICurrentUserContext _currentUserContext;

        public CreateOrderHandler(IOrderRepository orderRepository, ICurrentDateTimeProvider currentDateTimeProvider, ICurrentUserContext currentUserContext)
        {
            _orderRepository = orderRepository;
            _currentDateTimeProvider = currentDateTimeProvider;
            _currentUserContext = currentUserContext;
        }
        
        public async Task<CreateOrderOutputDto> Handle(CreateOrderInputDto request, CancellationToken cancellationToken)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

            var manager = await _orderRepository.GetManagerByIdAsync(_currentUserContext.Id,cancellationToken);
            if(manager == null)
                manager = ManagerFactory.GetCurrentUserAsManager(_currentUserContext);
            var order = OrderMapper.MapNewOrder(request, _currentDateTimeProvider, manager);
            
            _orderRepository.Add(order);

            await _orderRepository.SaveChangesAsync(cancellationToken);

            return new CreateOrderOutputDto()
            {
                Id = order.Id
            };
        }
    }
}