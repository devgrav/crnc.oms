using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Crnc.Oms.Security.WebApi.DTO
{
    public class UserItemDto
    {
        public Guid Id { get; set; }
        
        public string FullName { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }
        
        public string Login { get; set; }
        
        public string Password { get; set; }
        
        public string Phone { get; set; }
        
        public Guid RoleId { get; set; }
        
        public string Role { get; set; }
        
        public string PhotoBase64 { get; set; }
        
        public string PhotoMimeType { get; set; }
        
        public bool IsActive { get; set; }
    }
}
