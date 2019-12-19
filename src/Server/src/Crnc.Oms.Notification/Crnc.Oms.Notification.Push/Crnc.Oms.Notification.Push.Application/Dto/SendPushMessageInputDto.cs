using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Push.Application.Dto
{
    public class SendPushMessageInputDto
    {
        public Guid? MessageId { get; set; }
        
        public Guid ReceiverUserId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}