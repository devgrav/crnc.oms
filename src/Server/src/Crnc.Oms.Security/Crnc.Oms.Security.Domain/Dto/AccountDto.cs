using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Security.Domain.Dto
{
    ///<summary>
    /// Auth users data
    /// </summary>
    /// <example>
    ///{
    ///     "login": "admin",
    ///     "password": "111111"
    ///}
    /// </example>>
    public class AccountDto
    {
        /// <summary>
        /// User's Login 
        /// </summary>
        [Required]
        public string Login { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}