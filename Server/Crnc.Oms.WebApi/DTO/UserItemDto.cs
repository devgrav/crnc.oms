using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CrncOmsWeb.DTO
{
    public class UserItemDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        [Required]
        [Display(Name ="First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Phone]
        public string Phone { get; set; }

        public string Role { get; set; }

        public string PhotoBase64 { get; set; }

        public string PhotoMimeType { get; set; }

        public bool IsActive { get; set; }
    }
}
