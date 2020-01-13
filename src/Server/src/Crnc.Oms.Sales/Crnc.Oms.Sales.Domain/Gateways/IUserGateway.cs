using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Sales.Domain.Dto;

namespace Crnc.Oms.Sales.Domain.Gateways
{
    public interface IUserGateway
    {
        Task<UsersByRolesOutputDto> GetUsersByRolesAsync(UsersByRolesInputDto dto,
            CancellationToken cancellationToken = default);
    }
}