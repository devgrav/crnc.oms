using System;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Push.Application.Dto;
using Crnc.Oms.Notification.Push.Application.Services.Abstractions;
using Crnc.Oms.Notification.Push.Integration.Clients;
using Crnc.Oms.Notification.Push.Integration.Gateways;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;

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
        private readonly IHubContext<SignalRPushGateway, IPushNotificationClient> _pushHubContext;

        public PushNotificationsController(IPushNotificationService pushNotificationService, IHubContext<SignalRPushGateway, IPushNotificationClient> pushHubContext)
        {
            _pushNotificationService = pushNotificationService;
            _pushHubContext = pushHubContext;
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
            
            //var sendResult = await _pushNotificationService.SendAsync(inputDto,cancellationToken);
            await _pushHubContext.Clients.User(inputDto.ReceiverUserId.ToString()).ReceivePushMessageAsync(inputDto.ReceiverUserId.ToString(), inputDto.Message);

            var sendResult = new SendPushMessageOutputDto()
            {
                MessageId = inputDto.MessageId ?? new Guid()
            };

            return Ok(sendResult);
        }
    }
}