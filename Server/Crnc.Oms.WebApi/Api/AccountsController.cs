using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Crnc.Oms.Domain.Aggregates.Users;
using Crnc.Oms.Domain.IRepositories;
using Crnc.Oms.Infrastructure.CrossCutting;
using Crnc.Oms.Infrastructure.DataAccess.Exceptions;
using Crnc.Oms.WebApi.auth;
using Crnc.Oms.WebApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Crnc.Oms.WebApi.Api
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

            var jwt = GetToken();

            return Ok(new CurrentUserDto(){
                Login = user.Login,
                FullName = user.FullName,
                Role = user.Role.Title,
                Jwt = jwt
            });    
        }   

        private string GetToken()
        {
            var secretKey = AuthSettings.GetSymmetricSecurityKey(_authSettings.JwtBase64SymmetricKey);
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
 
            var now = DateTime.Now; 
            var tokenOptions = new JwtSecurityToken(
                issuer: AuthSettings.ISSUER,
                audience: AuthSettings.AUDIENCE,
                claims: new List<Claim>(),
                expires: now.AddSeconds(_authSettings.JwtLifetimeSeconds),
                signingCredentials: signinCredentials
            );
 
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return tokenString;
        }
    }
}