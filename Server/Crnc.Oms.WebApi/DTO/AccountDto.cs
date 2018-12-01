using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.WebApi.DTO
{
    public class AccountDto
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }


    }
}