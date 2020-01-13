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
using Crnc.Oms.Sales.Application.Helpers;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.Dto;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Sales.Application.Features.Orders.Commands
{
    public class EditOrder
        : IUseCaseCommandHandler<EditOrderInputDto, EmptyOutputDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserGateway _userGateway;
        private readonly INotificationGateway _notificationGateway;
        private readonly ICurrentDateTimeProvider _currentDateTimeProvider;
        private readonly ICurrentUserContext _userContext;
        private readonly ILogger<EditOrder> _logger;

        public EditOrder(IOrderRepository orderRepository, IUserGateway userGateway, INotificationGateway notificationGateway, 
            ICurrentDateTimeProvider currentDateTimeProvider, ICurrentUserContext userContext, ILogger<EditOrder> logger)
        {
            _orderRepository = orderRepository;
            _userGateway = userGateway;
            _notificationGateway = notificationGateway;
            _currentDateTimeProvider = currentDateTimeProvider;
            _userContext = userContext;
            _logger = logger;
        }
        
        public async Task<EmptyOutputDto> HandleAsync(EditOrderInputDto command, CancellationToken cancellationToken = default)
        {
            if(command == null)
                throw new ArgumentNullException(nameof(command));

            var order = await _orderRepository.FindByIdAsync(command.Id, cancellationToken);
            
            if(order == null)
                throw new MissingEntityException("Order not found");

            order = OrderMapper.MapExistedOrder(order, command);

            var oldStatus = order.Status;
            
            order.ChangeStatus(command.Status,_currentDateTimeProvider.GetNow());

            await _orderRepository.SaveChangesAsync(cancellationToken);

            if (oldStatus != order.Status)
            {
                await NotifyMainManagersIfStatusChangedAsync(order.Id, order.StatusDate, order.Status, _userContext.FullName, _userContext.Login, cancellationToken);
            }
            
            return new EmptyOutputDto();
        }

        private async Task NotifyMainManagersIfStatusChangedAsync(Guid orderId, DateTime changedDateTime, 
            OrderStatus newStatus, string userName, string userLogin, CancellationToken cancellationToken = default)
        {
            var usersByRolesInputDto = new UsersByRolesInputDto()
            {
                Roles = new List<string>()
                {
                    UserRoles.MainManager
                }
            };

            UsersByRolesOutputDto users = null;
            try
            {
                users = await _userGateway.GetUsersByRolesAsync(usersByRolesInputDto,cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not notify main managers, because occured error from user's data gateway. Error details: {e}");
                return;
            }

            if (users?.Items != null && users.Items.Any())
            {
                var mainManagers = users.Items;
                
                List<Task> notificationTasks = new List<Task>();
                foreach (var mainManager in mainManagers)
                {
                    var notifyUserInputDto = new NotifyUserInputDto()
                    {
                        UserId = mainManager.Id,
                        Message =
                            $"Status of order {orderId} changed to {EnumHelper.GetDescription(newStatus)} at {changedDateTime.ToStandartFormatWithTime()} by {userName} ({userLogin})"
                    };

                    notificationTasks.Add(_notificationGateway.NotifyUserAsync(notifyUserInputDto));
                }

                await Task.WhenAll(notificationTasks);
            }
        }
    }
}