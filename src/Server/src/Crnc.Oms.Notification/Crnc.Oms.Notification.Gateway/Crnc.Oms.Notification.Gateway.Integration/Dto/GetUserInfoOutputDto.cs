using System;

namespace Crnc.Oms.Notification.Gateway.Integration.Dto
{
    public class GetUserInfoOutputDto
    {
        public Guid UserId { get; set; }
        
        public string Email { get; set; }
    }
}