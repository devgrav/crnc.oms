using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Gateway.Application.Dto
{
    ///<summary>
    /// Notification data
    /// </summary>
    /// <example>
    ///{
    ///     "receiverUserId": "b5d75d01-4a8b-4b1f-889d-dd816b042eca",
    ///     "message": "Some message"
    ///}
    /// </example>
    public class SendAllChannelsNotificationInputDto
    {
        public Guid ReceiverUserId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}