using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Gateway.Integration.Dto
{
    public class SendPushInputDto
    { 
        public Guid MessageId { get; set; }

        public Guid ReceiverUserId { get; set; }

        public string Message { get; set; }

        public SendPushInputDto(Guid? messageId, Guid receiverUserId, string message)
        {
            MessageId = messageId ?? Guid.NewGuid();
            ReceiverUserId = receiverUserId;
            Message = message;
        }
    }
}