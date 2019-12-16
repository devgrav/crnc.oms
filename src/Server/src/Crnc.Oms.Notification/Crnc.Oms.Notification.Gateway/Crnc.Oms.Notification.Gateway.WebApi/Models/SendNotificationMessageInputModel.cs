using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Notification.Gateway.Application.Dto;

namespace Crnc.Oms.Notification.Gateway.WebApi.Models
{
    ///<summary>
    /// Sending notification data
    /// </summary>
    /// <example>
    ///{
    ///     "receiver": "receiver@mail.ru",
    ///     "message": "some message",
    ///     "channel": "email"
    ///}
    /// </example>
    public class SendNotificationMessageInputModel
    {
        /// <summary>
        /// Receiver's info, can be email or client id
        /// </summary>
        [Required]
        public string Receiver { get; set; }

        /// <summary>
        /// Message for sending
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// Channel
        /// </summary>
        public ChannelType Channel { get; set; }
    }
}