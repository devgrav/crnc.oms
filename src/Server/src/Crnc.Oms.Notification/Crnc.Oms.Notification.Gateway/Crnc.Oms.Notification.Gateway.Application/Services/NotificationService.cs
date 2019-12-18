using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Gateways;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Email.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Gateway.Application.Services
{
    public class NotificationService
        : INotificationService
    {
        private readonly IEmailGateway _emailGateway;
        private readonly IPushGateway _pushGateway; 
        private readonly IUserInfoGateway _userInfoGateway; 

        public NotificationService(IEmailGateway emailGateway, IPushGateway pushGateway, IUserInfoGateway userInfoGateway)
        {
            _emailGateway = emailGateway;
            _pushGateway = pushGateway;
            _userInfoGateway = userInfoGateway;
        }

        public async Task<SendEmailNotificationOutputDto> SendToEmailChannelAsync(SendEmailNotificationInputDto dto, CancellationToken cancellationToken = default)
        {
            var messageId = Guid.NewGuid();
            var response = await _emailGateway.SendEmailAsync(new SendEmailInputDto(messageId, dto.ReceiverEmail, dto.Message), cancellationToken);

            return new SendEmailNotificationOutputDto()
            {
                MessageId = response.MessageId
            };
        }

        public async Task<SendPushNotificationOutputDto> SendToPushChannelAsync(SendPushNotificationInputDto dto, CancellationToken cancellationToken = default)
        {
            var messageId = Guid.NewGuid();
            var response = await _pushGateway.SendPushAsync(new SendPushInputDto(messageId, dto.ReceiverUserId, dto.Message),cancellationToken);

            return new SendPushNotificationOutputDto()
            {
                MessageId = response.MessageId
            };
        }

        public async Task<SendAllChannelsNotificationOutputDto> SendToAllChannelsAsync(SendAllChannelsNotificationInputDto dto, CancellationToken cancellationToken = default)
        {
            var messageId = Guid.NewGuid();
            
            //Get email by user id
            var userInfo = await _userInfoGateway.GetUserInfoAsync(new GetUserInfoInputDto()
            {
                UserId = dto.ReceiverUserId
            });
            
            if(userInfo == null)
                throw new ArgumentNullException("Could not get user info for all channels notification");

            await _emailGateway.SendEmailAsync(new SendEmailInputDto(messageId,userInfo.Email, dto.Message), cancellationToken);
            await _pushGateway.SendPushAsync(new SendPushInputDto(messageId, dto.ReceiverUserId, dto.Message), cancellationToken);

            return new SendAllChannelsNotificationOutputDto()
            {
                MessageId = messageId
            };
        }
    }
}