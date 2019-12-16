using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using Crnc.Oms.Notification.Gateway.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Crnc.Oms.Notification.Gateway.WebApi.Controllers
{
    /// <summary>
    /// Management of notifications 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Send(SendNotificationMessageInputModel inputModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            
            var sendResult = await _notificationService.SendAsync(new SendNotificationMessageInputDto()
            {
                Channel = inputModel.Channel,
                Message = inputModel.Message,
                Receiver = inputModel.Receiver
            });

            return Ok(new SendNotificationOutputModel()
            {
                MessageId = sendResult.MessageId
            });
        }
    }
}