using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Dto;
using Crnc.Oms.Security.Domain.Repositories;
using Crnc.Oms.Security.Infrastructure.CrossCutting;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using Crnc.Oms.Security.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag.Annotations;

namespace Crnc.Oms.Security.WebApi.Controllers
{
    /// <summary>
    /// Authentication and authorization users 
    /// </summary>
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AccountsController
        : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly AuthSettings _authSettings;

        public AccountsController(IUserRepository userRepository, IOptions<AuthSettings> settings)
        {
            _userRepository = userRepository;
            _authSettings = settings.Value;
        }


        /// <summary>
        /// Authorize user
        /// </summary>
        /// <remarks>Returns user info with token in JWT format</remarks>
        /// <response code="200">Authorized.</response>
        /// <response code="400">Invalid auth data</response>
        [HttpPost("auth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Authenticate([FromBody]AccountDto account)
        {
            if(!ModelState.IsValid)
                return BadRequest();
            
            User user = null;
            
            try
            {
                user = await _userRepository.FindByLoginAsync(account.Login);
            }
            catch(MissingEntityException)
            {                
                return BadRequest("Not valid login or password");
            }

            if(!PasswordHelper.IsRightPassword(user.PasswordHash, user.PasswordSalt,account.Password))
                return BadRequest("Not valid login or password");

            if(!user.IsActive)
                return BadRequest("User is not active");

            var identity = GetIdentity(user);
            var jwt = GetToken(identity);

            return Ok(new CurrentUserDto(){
                Id = user.Id,
                Login = user.Login,
                FullName = user.FullName,
                Role = user.Role.Title,
                Jwt = jwt
            });    
        }   

        private ClaimsIdentity GetIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Title),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname,user.LastName),
                new Claim(ClaimTypes.Email,user.Email)
            };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
                
            return claimsIdentity;
        }

        private string GetToken(ClaimsIdentity identity)
        {
            var secretKey = _authSettings.SymmetricSecurityKey;
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
 
            var now = DateTime.Now; 
            var tokenOptions = new JwtSecurityToken(
                issuer: _authSettings.JwtIssuer,
                audience: _authSettings.JwtAudience,
                claims: identity.Claims,
                expires: now.AddSeconds(_authSettings.JwtLifetimeSeconds),
                signingCredentials: signinCredentials
            );
 
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return tokenString;
        }
    }
}