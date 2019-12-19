using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Gateway.Application.Dto
{
    public class SendAllChannelsNotificationInputDto
    {
        public Guid ReceiverUserId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}