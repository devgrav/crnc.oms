using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Email.Application.Dto
{
    ///<summary>
    /// Notification data
    /// </summary>
    /// <example>
    ///{
    ///     "messageId": "3e0a3ac5-0b6a-498f-925f-a97fddc43afd",
    ///     "senderEmail: "sender@email.ru",
    ///     "receiverEmail: "receiver@mail.ru",
    ///     "message": "Some message"
    ///}
    /// </example>
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