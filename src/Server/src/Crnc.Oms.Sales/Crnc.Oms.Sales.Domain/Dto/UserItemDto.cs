using System;

namespace Crnc.Oms.Sales.Domain.Dto
{
    public class UserItemDto
    {
        public Guid Id { get; set; }
        
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