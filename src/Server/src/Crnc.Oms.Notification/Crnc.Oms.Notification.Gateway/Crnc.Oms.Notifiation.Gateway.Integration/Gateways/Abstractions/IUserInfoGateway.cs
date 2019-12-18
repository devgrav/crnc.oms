using System.Threading.Tasks;
using Crnc.Oms.Notification.Email.Integration.Dto;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions
{
    public interface IUserInfoGateway
    {
        Task<GetUserInfoOutputDto> GetUserInfoAsync(GetUserInfoInputDto inputDto);
    }
}