using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Abstractions;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Gateway.Application.Services
{
    public class NotificationService
        : INotificationService
    {
        private readonly IEmailGateway _emailGateway;
        private readonly IPushGateway _pushGateway;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IEmailGateway emailGateway, IPushGateway pushGateway, ILogger<NotificationService> logger)
        {
            _emailGateway = emailGateway;
            _pushGateway = pushGateway;
            _logger = logger;
        }
        
        public async Task<SendNotificationMessageOutputDto> SendAsync(SendNotificationMessageInputDto dto, CancellationToken cancellationToken = default)
        {
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));
            
            if(string.IsNullOrWhiteSpace(dto.Message))
                throw new Exception($"Empty {nameof(dto.Message)}");
            
            if(string.IsNullOrWhiteSpace(dto.Receiver))
                throw new Exception($"Empty {nameof(dto.Receiver)}");

            var messageId = Guid.NewGuid();
            switch (dto.Channel)
            {
                case ChannelType.All:
                    LogSending(dto, messageId);
                    
                    await _emailGateway.SendEmailAsync(new SendEmailInputDto(messageId,dto.Receiver, dto.Message), cancellationToken);
                    await _pushGateway.SendPushAsync(new SendPushInputDto(messageId, dto.Receiver, dto.Message), cancellationToken);
                    
                    LogSent(dto, messageId);
                    break;
                case ChannelType.Email:
                    LogSending(dto, messageId);
                    
                    await _emailGateway.SendEmailAsync(new SendEmailInputDto(messageId, dto.Receiver, dto.Message),cancellationToken);
                    
                    LogSent(dto, messageId);
                    break;
                case ChannelType.Push:
                    LogSending(dto, messageId);
                    
                    await _pushGateway.SendPushAsync(new SendPushInputDto(messageId, dto.Receiver, dto.Message), cancellationToken);
                    
                    LogSent(dto, messageId);
                    break;
                default:
                    throw new InvalidOperationException("Not valid channel");
            }

            return new SendNotificationMessageOutputDto()
            {
                MessageId = messageId
            };
        }

        private void LogSending(SendNotificationMessageInputDto dto, Guid messageId)
        {
            _logger.LogInformation($"Message is sending  to channel: {dto.Channel} with id {messageId}. " +
                                   $"{nameof(dto.Receiver)}: {dto.Receiver}; {nameof(dto.Message)}: {dto.Message};");
        }
        
        private void LogSent(SendNotificationMessageInputDto dto, Guid messageId)
        {
            _logger.LogInformation($"Message sent to channel: {dto.Channel} with id {messageId}. " +
                                   $"{nameof(dto.Receiver)}: {dto.Receiver}; {nameof(dto.Message)}: {dto.Message};");
        }
    }
}