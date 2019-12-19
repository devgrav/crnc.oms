using System;

namespace Crnc.Oms.Notification.Gateway.Integration.Dto
{
    public class EmailMessageInputDto
    {
        public Guid MessageId { get; set; }
        
        public string Sender { get; set; }

        public string Receiver { get; set; }

        public string Message { get; set; }

        public EmailMessageInputDto(Guid? messageId, string sender, string receiver, string message)
        {
            MessageId = messageId ?? Guid.NewGuid();
            Message = message;
            Receiver = receiver;
            Sender = sender;
        }
    }
}