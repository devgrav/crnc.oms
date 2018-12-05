using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Domain.IRepositories;
using Crnc.Oms.WebApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Crnc.Oms.WebApi.Api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public RolesController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all user roles",
            Description = "Requires admin role",
            OperationId = "Get roles")]
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