using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrncOmsWeb.DTO
{
    public class UserGridItemDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }
    }
}
