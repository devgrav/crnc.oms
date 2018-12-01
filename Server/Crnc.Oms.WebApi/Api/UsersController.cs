using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crnc.Oms.Domain.IRepositories;
using CrncOmsWeb.DTO;
using Crnc.Oms.Domain.Aggregates.Users;
using UserEntity = Crnc.Oms.Domain.Aggregates.Users.User;
using System.ComponentModel.DataAnnotations;

namespace CrncOmsWeb.Api
{
    [Route("api/[controller]")]
    public class UsersController 
        : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/users
        [HttpGet]
        public IEnumerable<UserItemDto> Get()
        {
            var users = _userRepository.FindAll();
            return users.Select(u => new UserItemDto()
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
            }).ToList();           
        }

        // GET api/users/5
        [HttpGet("{id}")]
        public UserItemDto Get(Guid id)
        {
            var user = _userRepository.FindById(id);

            return new UserItemDto()
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
            };
        }

        // POST api/users
        [HttpPost]
        public IActionResult Post([FromBody]UserItemDto user)
        {
            if (ModelState.IsValid)
            {
                var entity = UserEntity.CreateNew(user.Login, user.Password,
                    user.FirstName, user.LastName, user.Email, user.Phone, null, !string.IsNullOrWhiteSpace(user.PhotoBase64)
                        && !string.IsNullOrWhiteSpace(user.PhotoMimeType)
                    ? new UserPhoto(user.PhotoBase64, user.PhotoMimeType) : null);
                _userRepository.Add(entity);

                return Ok();
            }
            else
                return BadRequest(ModelState);
        }

        // PUT api/users/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody]UserItemDto user)
        {
            if (ModelState.IsValid)
            {
                var entity = UserEntity.CreateExisted(id, user.Login, user.Password,
                    user.FirstName, user.LastName, user.Email, new Role(user.Role), 
                    user.IsActive, user.Phone, !string.IsNullOrWhiteSpace(user.PhotoBase64) 
                        && !string.IsNullOrWhiteSpace(user.PhotoMimeType)
                    ? new UserPhoto(user.PhotoBase64, user.PhotoMimeType) : null);

                _userRepository.Save(entity);

                return Ok();
            }
            else
                return BadRequest(ModelState);
        }

        // DELETE api/users/5
        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {            
            _userRepository.Delete(id);
        }
    }
}
