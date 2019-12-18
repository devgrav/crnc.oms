using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Application.Dto;

namespace Crnc.Oms.Notification.Gateway.Application.Services.Abstractions
{
    public interface INotificationService
    {
        Task<SendEmailNotificationOutputDto> SendToEmailChannelAsync(SendEmailNotificationInputDto dto, CancellationToken cancellationToken = default);
        
        Task<SendPushNotificationOutputDto> SendToPushChannelAsync(SendPushNotificationInputDto dto, CancellationToken cancellationToken = default);
        
        Task<SendAllChannelsNotificationOutputDto> SendToAllChannelsAsync(SendAllChannelsNotificationInputDto dto, CancellationToken cancellationToken = default);
    }
}