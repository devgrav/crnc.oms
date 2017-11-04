using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crnc.Oms.Domain.IRepositories;
using CrncOmsWeb.DTO;
using Crnc.Oms.Domain.Aggregates.Users;
using UserEntity = Crnc.Oms.Domain.Aggregates.Users.User;

namespace CrncOmsWeb.Api
{
    [Route("api/[controller]")]
    public class UsersController 
        : Controller
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
                Role = u.Role.Title,
                PhotoBase64 = u.Photo?.ContentBase64,
                PhotoMimeType = u.Photo?.MimeType,
                IsActive = u.IsActive
            }).ToList();           
        }

        // GET api/users/5
        [HttpGet("{id}")]
        public UserItemDto Get(int id)
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
        public void Post([FromBody]UserItemDto user)
        {
            var entity = UserEntity.CreateNew(user.Login, user.Password, 
                user.FirstName, user.LastName, user.Email, user.Phone);
            _userRepository.Add(entity);
        }

        // PUT api/users/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]UserItemDto user)
        {
            var entity = UserEntity.CreateExisted(id,user.Login, user.Password, 
                user.FirstName, user.LastName, user.Email, new Role(user.Role), user.IsActive, user.Phone);

            _userRepository.Save(entity);
        }

        // DELETE api/users/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {            
            _userRepository.Delete(id);
        }
    }
}
