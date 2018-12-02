using Crnc.Oms.Domain.Aggregates.Users;
using Crnc.Oms.Domain.IRepositories;
using Crnc.Oms.Infrastructure.CrossCutting;
using Crnc.Oms.Infrastructure.DataAccess.Exceptions;
using Crnc.Oms.WebApi.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Crnc.Oms.WebApi.Api
{
    [Route("api/[controller]")]
    public class AccountsController
        : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AccountsController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        [HttpPost("auth")]
        public IActionResult Authentificate([FromBody]AccountDto account)
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

            return Ok(new CurrentUserDto(){
                Login = user.Login,
                FullName = user.FullName,
                Role = user.Role.Title
            });    
        }   
    }
}