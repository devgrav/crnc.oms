using System;

namespace Crnc.Oms.Notification.Push.Client.Auth
{
    public class AuthUserDto
    {
        public string Login { get; set; }

        public string FullName { get; set; }    
        
        public string Role { get; set; }  

        public string Jwt { get; set; } 
    }
}