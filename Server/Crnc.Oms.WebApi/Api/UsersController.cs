using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crnc.Oms.Domain.IRepositories;
using CrncOmsWeb.DTO;

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
                PhotoBase64 = u.Photo.ContentBase64,
                PhotoMimeType = u.Photo.MimeType,
                IsActive = u.IsActive
            }).ToList();           
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
