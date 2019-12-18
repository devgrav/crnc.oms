using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;
using Crnc.Oms.Notification.Push.Application.Dto;
using Crnc.Oms.Notification.Push.Application.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace Crnc.Oms.Notification.Push.WebApi.Controllers
{
    /// <summary>
    /// Management of notifications 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PushNotificationsController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;

        public PushNotificationsController(IPushNotificationService pushNotificationService)
        {
            _pushNotificationService = pushNotificationService;
        }

        /// <summary>
        /// Send push
        /// </summary>
        /// <response code="200">Push sent</response>
        /// <response code="400">Sent data is not valid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send([FromBody]SendPushMessageInputDto inputDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var sendResult = await _pushNotificationService.SendAsync(inputDto,cancellationToken);

            return Ok(sendResult);
        }
    }
}