using System;

namespace Crnc.Oms.Notification.Gateway.Integration.Dto
{
    public class PushMessageInputDto
    {
        public Guid MessageId { get; set; }

        public Guid ReceiverUserId { get; set; }

        public string Message { get; set; }

        public PushMessageInputDto(Guid? messageId, Guid receiverUserId, string message)
        {
            MessageId = messageId ?? Guid.NewGuid();
            ReceiverUserId = receiverUserId;
            Message = message;
        }
    }
}