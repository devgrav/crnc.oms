using System;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Notification.Email.Application.Dto;

namespace Crnc.Oms.Notification.Email.WebApi.Models
{
    ///<summary>
    /// Sending email data
    /// </summary>
    public class SendEmailMessageInputModel
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