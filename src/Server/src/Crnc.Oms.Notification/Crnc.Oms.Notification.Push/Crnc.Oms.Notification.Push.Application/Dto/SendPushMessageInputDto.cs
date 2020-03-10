using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Push.Application.Dto
{
    ///<summary>
    /// Notification data
    /// </summary>
    /// <example>
    ///{
    ///     "messageId": "3e0a3ac5-0b6a-498f-925f-a97fddc43afd",
    ///     "receiverUserId: "5ca0363e-5974-4685-849f-195d6c1e1d79",
    ///     "message": "Some message"
    ///}
    /// </example>
    public class SendPushMessageInputDto
    {
        public Guid MessageId { get; set; }
        
        public Guid ReceiverUserId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}