using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Gateway.Application.Dto
{
    ///<summary>
    /// Notification data
    /// </summary>
    /// <example>
    ///{
    ///     "receiverEmail": "some@email.ru",
    ///     "message": "Some message"
    ///}
    /// </example>
    public class SendEmailNotificationInputDto
    {
        [Required]
        [EmailAddress]
        public string ReceiverEmail { get; set; }

        [Required]
        public string Message { get; set; }
    }
}