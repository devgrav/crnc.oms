using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.Aggregates.Users
{
    /// <summary>
    /// User of application
    /// </summary>
    [Table("Users")]
    public class User
        : DomainEntity, IAggregateRoot
    {
        #region Constructors and Factories
        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="login">Username/login</param>
        /// <param name="passwordHash">Hash of password</param>
        /// <param name="firstName">FirstName</param>
        /// <param name="lastName">LastName</param>
        /// <returns></returns>
        public static User CreateNew(string login, string passwordHash, 
            string firstName, string lastName, string email, string phone=null, Role role=null, UserPhoto photo = null)
        {
            return new User()
            {
                Login = login,
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = role,
                Phone = phone,
                Photo = photo,
                IsActive = true
            };
        }

        public static User CreateExisted(int id,string login, string passwordHash,
            string firstName, string lastName, string email, string phone = null, Role role = null, UserPhoto photo = null)
        {
            return new User()
            {
                Id = id,
                Login = login,
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = role,
                Phone = phone,
                Photo = photo,
                IsActive = true
            };
        }

        protected User()
        {

        } 
        #endregion

        #region Properties

        /// <summary>
        /// User name
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Login { get; private set; }

        /// <summary>
        /// Password hash
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Full name
        /// </summary>        
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// First name
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string FirstName { get; private set; }

        /// <summary>
        /// Last name
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string LastName { get; private set; }

        /// <summary>
        /// Email address
        /// </summary>
        [MaxLength(200)]
        public string Email { get; private set; }

        [MaxLength(200)]
        public string Phone { get; set; }

        /// <summary>
        /// Property of active user
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Photo of user
        /// </summary>
        public virtual UserPhoto Photo { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        public virtual Role Role { get; private set; }

        #endregion

        #region Behavior

        /// <summary>
        /// Change password (hash of password)
        /// </summary>
        /// <param name="passwordHash">password's hash</param>
        public void ChangePassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentNullException("New password is empty ");

            PasswordHash = passwordHash;
        }

        /// <summary>
        /// Change first name
        /// </summary>
        /// <param name="firstName">new first name</param>
        public void ChangeFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentNullException("Changed first name is empty");

            FirstName = firstName;
        }

        /// <summary>
        /// Change last name
        /// </summary>
        /// <param name="lastName">new last name</param>
        public void ChangeLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentNullException("Changed last name is empty");

            LastName = lastName;
        }
        /// <summary>
        /// Change email
        /// </summary>
        /// <param name="email">email</param>
        public void ChangeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException("Changed email is empty");

            Email = email;
        }

        /// <summary>
        /// Make user disactive
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Make user active
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        #endregion
    }
}
