using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Security.WebApi.DTO
{
    /// <summary>
    /// Account's data for auth 
    /// </summary>
    public class AccountDto
    {
        /// <summary>
        /// User's Login 
        /// </summary>
        /// <example>admin</example>
        [Required]
        public string Login { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        /// <example>111111</example>
        [Required]
        public string Password { get; set; }


    }
}