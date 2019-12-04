using System;
using System.ComponentModel.DataAnnotations;

namespace Crnc.Oms.Security.WebApi.DTO
{
    ///<summary>
    /// User data
    /// </summary>
    /// <example>
    ///{
    ///     "firstName": "Adam",
    ///     "lastName": "Smith",
    ///     "email": "adamsmith@crnc.com",
    ///     "login": "adamsmith",
    ///     "password": 1111112,
    ///     "phone": 9151242312,
    ///     "roleId": "f1ba72d8-5ebc-4cc4-8b31-eaa0baa87293",
    ///     "role": "Main manager",
    ///     "isActive": true
    ///}
    /// </example>>
    public class SaveUserDto
    {
        /// <summary>
        /// First name of user
        /// </summary>
        /// <example>Adam</example>
        [Required]
        [Display(Name ="First name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of user
        /// </summary>
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        /// <summary>
        /// Email of user
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Email of user
        /// </summary>
        [Required]
        public string Login { get; set; }

        /// <summary>
        /// Password of user
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Mobile phone of user
        /// </summary>
        [Phone]
        public string Phone { get; set; }

        /// <summary>
        /// Id of user's role
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Photo in base64
        /// </summary>
        public string PhotoBase64 { get; set; }

        /// <summary>
        /// Photo mime type
        /// </summary>
        public string PhotoMimeType { get; set; }

        /// <summary>
        /// Activity type
        /// </summary>
        public bool IsActive { get; set; }
    }
}