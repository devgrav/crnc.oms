using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Application.Dto;

namespace Crnc.Oms.Notification.Push.Application.Services.Abstractions
{
    public interface IPushNotificationService
    {
        Task<SendPushMessageOutputDto> SendAsync(SendPushMessageInputDto dto, CancellationToken cancellationToken = default);
    }
}