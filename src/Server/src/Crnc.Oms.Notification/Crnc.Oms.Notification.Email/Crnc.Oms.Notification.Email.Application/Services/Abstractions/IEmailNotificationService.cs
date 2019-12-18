using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Email.Application.Dto;

namespace Crnc.Oms.Notification.Email.Application.Services.Abstractions
{
    public interface IEmailNotificationService
    {
        Task<SendEmailMessageOutputDto> SendAsync(SendEmailMessageInputDto dto, CancellationToken cancellationToken = default);
    }
}