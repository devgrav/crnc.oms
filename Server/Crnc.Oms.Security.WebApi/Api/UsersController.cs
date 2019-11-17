using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crnc.Oms.Security.Domain.IRepositories;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using UserEntity = Crnc.Oms.Security.Domain.Aggregates.Users.User;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Security.Infrastructure.CrossCutting;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using Crnc.Oms.Security.WebApi.Authorization;
using Crnc.Oms.Security.WebApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSwag.Annotations;

namespace Crnc.Oms.Security.WebApi.Api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class UsersController 
        : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <remarks>Requires admin role</remarks>
        /// <response code="200">Returns users.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserItemDto>),StatusCodes.Status200OK)]
        public ActionResult<List<UserItemDto>> Get()
        {
            var users = _userRepository.FindAll();
            return Ok(users.Select(u => new UserItemDto()
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                Email = u.Email,
                Password = u.PasswordHash,
                Login = u.Login,
                Phone = u.Phone,
                RoleId = u.Role.Id,
                Role = u.Role.Title,
                PhotoBase64 = u.Photo?.ContentBase64,
                PhotoMimeType = u.Photo?.MimeType,
                IsActive = u.IsActive
            }).ToList());           
        }

        /// <summary>
        /// Get users by Id
        /// </summary>
        /// <remarks>Requires admin role</remarks>
        /// <response code="200">Returns users.</response>
        /// <response code="404">Not found user.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserItemDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(Guid id)
        {
            try
            {
                var user = _userRepository.FindById(id);

                return Ok(new UserItemDto()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Password = user.PasswordHash,
                    Login = user.Login,
                    Phone = user.Phone,
                    Role = user.Role.Title,
                    PhotoBase64 = user.Photo?.ContentBase64,
                    PhotoMimeType = user.Photo?.MimeType,
                    IsActive = user.IsActive
                });
            }
            catch (MissingEntityException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <remarks>Requires admin role</remarks>
        /// <response code="200">User has created</response>
        /// <response code="400">User is not valid.</response>
        [HttpPost]
        [OpenApiOperation("Create user", "Create new user", "Requires admin role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromBody]UserItemDto user)
        {
            if (ModelState.IsValid)
            {
                var password = PasswordHelper.GetHash(user.Password);

                var entity = UserEntity.CreateNew(user.Login, password.Hash, password.Salt,
                    user.FirstName, user.LastName, user.Email, user.Phone, null, !string.IsNullOrWhiteSpace(user.PhotoBase64)
                        && !string.IsNullOrWhiteSpace(user.PhotoMimeType)
                    ? new UserPhoto(user.PhotoBase64, user.PhotoMimeType) : null);
                _userRepository.Add(entity);

                return Ok();
            }
            else
                return BadRequest(ModelState);
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <remarks>Requires admin role</remarks>
        /// <response code="200">User has updated</response>
        /// <response code="400">User is not valid.</response>
        /// <response code="404">User has not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Put(Guid id, [FromBody]UserItemDto user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var password = PasswordHelper.GetHash(user.Password);

                    var entity = UserEntity.CreateExisted(id, user.Login, password.Hash, password.Salt,
                        user.FirstName, user.LastName, user.Email, new Role(user.Role), 
                        user.IsActive, user.Phone, !string.IsNullOrWhiteSpace(user.PhotoBase64) 
                                                   && !string.IsNullOrWhiteSpace(user.PhotoMimeType)
                            ? new UserPhoto(user.PhotoBase64, user.PhotoMimeType) : null);

                    _userRepository.Save(entity);

                    return Ok();
                }
                catch (MissingEntityException)
                {
                    return NotFound();
                }
            }
            
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <remarks>Requires admin role</remarks>
        /// <response code="200">User has deleted</response>
        /// <response code="404">User has not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _userRepository.Delete(id);
                
                return Ok();
            }
            catch (MissingEntityException)
            {
                return NotFound();
            }
        }
    }
}
