using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Application.Dto;

namespace Crnc.Oms.Notification.Gateway.Application.Services.Abstractions
{
    public interface INotificationService
    {
        Task<SendNotificationToUserOutputDto> SendNotificationToUserAsync(SendToNotificationUserInputDto dto, CancellationToken cancellationToken = default);
    }
}