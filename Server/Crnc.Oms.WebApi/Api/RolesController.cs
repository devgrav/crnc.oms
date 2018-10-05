using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Domain.IRepositories;
using Crnc.Oms.WebApi.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crnc.Oms.WebApi.Api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class RolesController : Controller
    {
        private readonly IUserRepository _userRepository;

        public RolesController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IEnumerable<TextValueDto> Get()
        {
            return _userRepository.GetRoles().Select(r => new TextValueDto
            {
                Value = r.Id,
                Text = r.Title
            });
        }
    }
}