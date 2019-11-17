using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.IRepositories;
using Crnc.Oms.Security.Infrastructure.CrossCutting;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using Crnc.Oms.Security.WebApi.Authorization;
using Crnc.Oms.Security.WebApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag.Annotations;

namespace Crnc.Oms.Security.WebApi.Api
{
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


        [HttpPost("auth")]
        [OpenApiOperation("Authenticate token","Authenticate user","Returns user info with token in JWT format")]                
        public IActionResult Authenticate([FromBody]AccountDto account)
        {
            if(!ModelState.IsValid)
                return BadRequest();
            
            User user = null;
            
            try
            {
                user = _userRepository.FindByLogin(account.Login);
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
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Title)
            };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
                
            return claimsIdentity;
        }

        private string GetToken(ClaimsIdentity identity)
        {
            var secretKey = AuthSettings.GetSymmetricSecurityKey(_authSettings.JwtBase64SymmetricKey);
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
 
            var now = DateTime.Now; 
            var tokenOptions = new JwtSecurityToken(
                issuer: AuthSettings.ISSUER,
                audience: AuthSettings.AUDIENCE,
                claims: identity.Claims,
                expires: now.AddSeconds(_authSettings.JwtLifetimeSeconds),
                signingCredentials: signinCredentials
            );
 
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return tokenString;
        }
    }
}