using System;

namespace Crnc.Oms.Notification.Push.Client
{
    public class PushMessageInputDto
    {
        public Guid MessageId { get; set; }

        public Guid ReceiverUserId { get; set; }

        public string Message { get; set; }
    }
}