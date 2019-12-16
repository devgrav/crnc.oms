using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
using Crnc.Oms.Notification.Gateway.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

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

        /// <summary>
        /// Send notification
        /// </summary>
        /// <response code="200">Notification sent to channel</response>
        /// <response code="400">Sending data is not valid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send([FromBody]SendNotificationMessageInputModel inputModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
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