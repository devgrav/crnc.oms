using System;

namespace Crnc.Oms.Security.Domain.Dto
{
    public class CurrentUserDto
    {
        public Guid Id { get; set; }
        
        public string Login { get; set; }

        public string FullName { get; set; }    
        
        public string Role { get; set; }  

        public string Jwt { get; set; } 
    }
}