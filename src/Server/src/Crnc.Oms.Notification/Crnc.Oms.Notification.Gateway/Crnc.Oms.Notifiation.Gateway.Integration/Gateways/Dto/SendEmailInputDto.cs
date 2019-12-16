using System;

namespace Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto
{
    public class SendEmailInputDto
    {
        public Guid MessageId { get; set; }
        public string Sender { get; set; }

        public string Receiver { get; set; }

        public string Message { get; set; }

        public SendEmailInputDto(Guid messageId, string receiver, string message)
        {
            MessageId = messageId;
            Message = message;
            Receiver = receiver;
            Sender = "notifications@crnc.ru";
        }
    }
}