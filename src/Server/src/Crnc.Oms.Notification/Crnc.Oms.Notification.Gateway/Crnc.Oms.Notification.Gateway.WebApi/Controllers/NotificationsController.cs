using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Application.Dto;
using Crnc.Oms.Notification.Gateway.Application.Exceptions;
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
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Send notification to user
        /// </summary>
        /// <response code="200">Notification sent to user</response>
        /// <response code="400">Sending data is not valid.</response>
        [HttpPost("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendToUser([FromBody]SendNotificationToUserInputDto inputDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            SendNotificationToUserOutputDto outputDto = null;
            try
            {
                outputDto = await _notificationService.SendNotificationToUserAsync(inputDto);
            }
            catch (BadNotificationDataException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(outputDto);
        }
    }
}