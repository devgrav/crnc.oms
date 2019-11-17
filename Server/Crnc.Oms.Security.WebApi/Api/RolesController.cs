using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.IRepositories;
using Crnc.Oms.Security.WebApi.Authorization;
using Crnc.Oms.Security.WebApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Crnc.Oms.Security.WebApi.Api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class RolesController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public RolesController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all user roles
        /// </summary>
        /// <remarks>Requires admin role</remarks>
        /// <response code="200">Returns roles.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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