using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Email.Integration.Dto
{
    public class SendEmailMessageInputDto
    {
        public Guid? MessageId { get; set; }
        
        [Required]
        [EmailAddress]
        public string Sender { get; set; }
        
        [Required]
        [EmailAddress]
        public string Receiver { get; set; }

        [Required]
        public string Message { get; set; }
    }
}