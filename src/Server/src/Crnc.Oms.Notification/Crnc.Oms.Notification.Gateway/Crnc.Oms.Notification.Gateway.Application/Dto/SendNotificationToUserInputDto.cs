using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Notification.Gateway.Application.Dto
{
    ///<summary>
    /// Notification data
    /// </summary>
    /// <example>
    ///{
    ///     "userId": "2a89985f-f013-4f2a-9545-395efb43a142",
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