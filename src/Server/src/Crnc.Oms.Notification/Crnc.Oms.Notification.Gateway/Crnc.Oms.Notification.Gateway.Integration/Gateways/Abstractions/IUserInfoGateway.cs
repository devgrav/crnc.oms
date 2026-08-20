using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Notification.Gateway.Integration.Dto;

namespace Crnc.Oms.Notification.Gateway.Integration.Gateways.Abstractions
{
    public interface IUserInfoGateway
    {
        Task<GetUserInfoOutputDto> GetUserInfoAsync(GetUserInfoInputDto inputDto, CancellationToken cancellationToken = default);
    }
}