using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto;
using Crnc.Oms.Notification.Email.Application.Dto;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using Crnc.Oms.Notification.Email.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace Crnc.Oms.Notification.Email.WebApi.Controllers
{
    /// <summary>
    /// Management of notifications 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class EmailNotificationsController : ControllerBase
    {
        private readonly IEmailNotificationService _emailNotificationService;

        public EmailNotificationsController(IEmailNotificationService emailNotificationService)
        {
            _emailNotificationService = emailNotificationService;
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <response code="200">Email sent</response>
        /// <response code="400">Sent data is not valid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send([FromBody]SendEmailMessageInputModel messageInputModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new SendEmailMessageInputDto()
            {
                MessageId = messageInputModel.MessageId,
                Message = messageInputModel.Message,
                Receiver = messageInputModel.Receiver,
                Sender = messageInputModel.Sender
            };
            
            var sendResult = await _emailNotificationService.SendAsync(dto,cancellationToken);

            return Ok(new SendEmailMessageOutputModel()
            {
                MessageId = sendResult.MessageId
            });
        }
    }
}