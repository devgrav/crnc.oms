using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Gateway.Application.Dto
{
    ///<summary>
    /// Notification data
    /// </summary>
    /// <example>
    ///{
    ///     "userId": "b6ba35b2-adff-43a6-9cd7-b408240a6d6f",
    ///     "message": "Some message"
    ///}
    /// </example>
    public class SendNotificationToUserInputDto
    {
        public Guid UserId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}