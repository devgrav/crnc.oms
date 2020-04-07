using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Application.Exceptions;
using Crnc.Oms.Sales.Application.Factories;
using Crnc.Oms.Sales.Application.Features.Orders.Dto;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Sales.Application.Features.Orders.Commands
{
    public class EditOrderHandler
        : IUseCaseCommandHandler<EditOrderInputDto, EmptyOutputDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentDateTimeProvider _currentDateTimeProvider;
        private readonly ICurrentUserContext _userContext;

        public EditOrderHandler(IOrderRepository orderRepository, 
            ICurrentDateTimeProvider currentDateTimeProvider, ICurrentUserContext userContext)
        {
            _orderRepository = orderRepository;
            _currentDateTimeProvider = currentDateTimeProvider;
            _userContext = userContext;
        }

        public async Task<EmptyOutputDto> Handle(EditOrderInputDto request, CancellationToken cancellationToken)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

            var manager = await _orderRepository.GetManagerByIdAsync(_userContext.Id,cancellationToken);
            if(manager == null)
                manager = ManagerFactory.GetCurrentUserAsManager(_userContext);
            
            var order = await _orderRepository.FindByIdAsync(request.Id, cancellationToken);
            
            if(order == null)
                throw new MissingEntityException("Order not found");

            order = OrderMapper.MapExistedOrder(order, request);

            order.ChangeStatus(Enumeration.FromValue<OrderStatus>((int)request.Status),_currentDateTimeProvider.GetNow(), manager);

            await _orderRepository.SaveChangesAsync(cancellationToken);
            
            return new EmptyOutputDto();
        }
    }
}