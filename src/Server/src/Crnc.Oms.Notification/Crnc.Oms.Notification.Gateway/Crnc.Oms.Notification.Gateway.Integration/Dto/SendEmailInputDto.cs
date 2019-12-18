using System;

namespace Crnc.Oms.Notification.Gateway.Integration.Dto
{
    public class SendEmailInputDto
    {
        public Guid MessageId { get; set; }
        
        public string SenderEmail { get; set; }

        public string ReceiverEmail { get; set; }

        public string Message { get; set; }

        public SendEmailInputDto(Guid messageId, string receiverEmail, string message)
        {
            MessageId = messageId;
            Message = message;
            ReceiverEmail = receiverEmail;
            SenderEmail = "notifications@crnc.ru";
        }

        public SendEmailInputDto()
        {
            
        }
    }
}