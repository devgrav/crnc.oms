using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Gateway.Application.Dto
{
    ///<summary>
    /// Sending notification data
    /// </summary>
    public class SendEmailNotificationInputDto
    {
        [Required]
        [EmailAddress]
        public string ReceiverEmail { get; set; }

        [Required]
        public string Message { get; set; }
    }
}