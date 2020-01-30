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
        
        public async Task<EmptyOutputDto> HandleAsync(EditOrderInputDto command, CancellationToken cancellationToken = default)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));

            var manager = ManagerFactory.GetCurrentUserAsManager(_userContext);
            
            var order = await _orderRepository.FindByIdAsync(command.Id, cancellationToken);
            
            if(order == null)
                throw new MissingEntityException("Order not found");

            order = OrderMapper.MapExistedOrder(order, command);

            order.ChangeStatus(command.Status,_currentDateTimeProvider.GetNow(), manager);

            await _orderRepository.SaveChangesAsync(cancellationToken);
            
            return new EmptyOutputDto();
        }
    }
}