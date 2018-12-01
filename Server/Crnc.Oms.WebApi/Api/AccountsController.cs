using Crnc.Oms.Domain.IRepositories;
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


        [HttpPost("/auth")]
        public IActionResult Authentificate(AccountDto account)
        {
            if(!ModelState.IsValid)
                return BadRequest();
            
            var user = _userRepository.FindByLogin(account.Login);

            return Ok();    
        }   
    }
}