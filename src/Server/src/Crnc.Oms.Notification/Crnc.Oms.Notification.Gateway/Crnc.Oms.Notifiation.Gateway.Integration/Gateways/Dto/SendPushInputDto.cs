using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notifiation.Gateway.Integration.Gateways.Dto
{
    public class SendPushInputDto
    { 
        public Guid MessageId { get; set; }
        
        public string Sender { get; set; }

        public string Receiver { get; set; }
        
        public string Message { get; set; }

        public SendPushInputDto(Guid messageId,  string receiver, string message)
        {
            MessageId = messageId;
            Message = message;
            Receiver = receiver;
            Sender = "crnc_notifications";
        }
    }
}