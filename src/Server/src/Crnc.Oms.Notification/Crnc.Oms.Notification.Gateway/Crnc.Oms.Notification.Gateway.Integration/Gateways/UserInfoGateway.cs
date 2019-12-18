using System;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Email.Integration.Dto;
using Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways
{
    public class UserInfoGateway
        : IUserInfoGateway
    {
        public Task<GetUserInfoOutputDto> GetUserInfoAsync(GetUserInfoInputDto inputDto)
        {
            return Task.FromResult(new GetUserInfoOutputDto()
            {
                Email = "someUser@mail.ru",
                UserId = Guid.NewGuid()
            });
        }
    }
}