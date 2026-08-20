using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Gateways;
using Crnc.Oms.Notification.Gateway.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Exceptions;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using Crnc.Oms.Notification.Gateway.Integration.Exceptions;
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

        public async Task<SendNotificationToUserOutputDto> SendNotificationToUserAsync(SendToNotificationUserInputDto dto, CancellationToken cancellationToken = default)
        {
            var messageId = Guid.NewGuid();
            
            //Get email by user id
            GetUserInfoOutputDto userInfo = null;

            try
            {
                userInfo = await _userInfoGateway.GetUserInfoAsync(new GetUserInfoInputDto()
                {
                    UserId = dto.UserId
                }, cancellationToken);
            }
            catch (MissingUserException)
            {
                throw new BadNotificationDataException($"User with id {dto.UserId} have not found");
            }

            if(userInfo == null)
                throw new ArgumentNullException("Could not get user info for notification");
            
            if(string.IsNullOrWhiteSpace(userInfo.Email))
                throw new BadNotificationDataException($"User with id {dto.UserId} have not email");

            await _emailGateway.SendEmailAsync(new SendEmailInputDto(messageId,userInfo.Email, dto.Message), cancellationToken);
            await _pushGateway.SendPushAsync(new SendPushInputDto(messageId, dto.UserId, dto.Message), cancellationToken);

            return new SendNotificationToUserOutputDto()
            {
                MessageId = messageId
            };
        }
    }
}