using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Services.Abstractions;
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
        /// Send email notification
        /// </summary>
        /// <response code="200">Notification sent to email channel</response>
        /// <response code="400">Sending data is not valid.</response>
        [HttpPost("email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendEmail([FromBody]SendEmailNotificationInputDto inputDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sendResult = await _notificationService.SendToEmailChannelAsync(inputDto);

            return Ok(sendResult);
        }
        
        /// <summary>
        /// Send push notification
        /// </summary>
        /// <response code="200">Notification sent to push channel</response>
        /// <response code="400">Sending data is not valid.</response>
        [HttpPost("push")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendPush([FromBody]SendPushNotificationInputDto inputDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sendResult = await _notificationService.SendToPushChannelAsync(inputDto);

            return Ok(sendResult);
        }
        
        /// <summary>
        /// Send notification to all channels
        /// </summary>
        /// <response code="200">Notification sent to all channels</response>
        /// <response code="400">Sending data is not valid.</response>
        [HttpPost("allChannels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendToAllChannels([FromBody]SendAllChannelsNotificationInputDto inputDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var sendResult = await _notificationService.SendToAllChannelsAsync(inputDto);

            return Ok(sendResult);
        }
    }
}