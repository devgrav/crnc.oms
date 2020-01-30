using System;

namespace Crnc.Oms.Security.Domain.Dto
{
    public class UserShortInfoItemDto
    {
        public Guid Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string FullName { get; set; }

        public string Email { get; set; }
        
        public string Login { get; set; }
        
        public string Password { get; set; }
        
        public string Phone { get; set; }
        
        public Guid RoleId { get; set; }
        
        public string Role { get; set; }

        public bool IsActive { get; set; }
    }
}
