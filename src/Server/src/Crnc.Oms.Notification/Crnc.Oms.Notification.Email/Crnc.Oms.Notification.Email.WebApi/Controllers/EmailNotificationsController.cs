using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Email.Application.Dto;
using Crnc.Oms.Notification.Email.Application.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Crnc.Oms.Notification.Email.WebApi.Controllers
{
    /// <summary>
    /// Management of email notifications 
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Send([FromBody]SendEmailMessageInputDto inputDto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sendResult = await _emailNotificationService.SendAsync(inputDto,cancellationToken);

            return Ok(sendResult);
        }
    }
}