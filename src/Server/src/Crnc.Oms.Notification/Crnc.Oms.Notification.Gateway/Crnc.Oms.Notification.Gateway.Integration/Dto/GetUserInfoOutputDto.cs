using System;

namespace Crnc.Oms.Notification.Email.Integration.Dto
{
    public class GetUserInfoOutputDto
    {
        public Guid UserId { get; set; }
        
        public string Email { get; set; }
    }
}