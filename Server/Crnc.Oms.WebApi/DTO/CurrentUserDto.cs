using System;

namespace Crnc.Oms.WebApi.DTO
{
    public class CurrentUserDto
    {
        public string Login { get; set; }

        public string FullName { get; set; }    
        
        public string Role { get; set; }  

        public string Jwt { get; set; } 
    }
}