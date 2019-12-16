using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Notification.Gateway.Application.Dto;

namespace Crnc.Oms.Notification.Gateway.WebApi.Models
{
    ///<summary>
    /// Sending notification data
    /// </summary>
    /// <example>
    ///{
    ///     "Receiver": "receiver@mail.ru",
    ///     "Message": "Some message",
    ///     "Channel: "Email"
    ///}
    /// </example>
    /// <example>
    ///{
    ///     "Receiver": "receiverId",
    ///     "Message": "Some message",
    ///     "Channel: "Push"
    ///}
    /// </example>
    /// <example>
    ///{
    ///     "Receiver": "receiverId",
    ///     "Message": "Some message",
    ///     "Channel: "All"
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
        [Required]
        public ChannelType Channel { get; set; }
    }
}