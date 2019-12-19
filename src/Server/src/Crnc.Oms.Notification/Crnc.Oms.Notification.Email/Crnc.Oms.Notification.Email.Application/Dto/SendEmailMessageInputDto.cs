using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Email.Application.Dto
{
    public class SendEmailMessageInputDto
    {
        public Guid? MessageId { get; set; }
        
        [Required]
        [EmailAddress]
        public string SenderEmail { get; set; }
        
        [Required]
        [EmailAddress]
        public string ReceiverEmail { get; set; }

        [Required]
        public string Message { get; set; }
    }
}