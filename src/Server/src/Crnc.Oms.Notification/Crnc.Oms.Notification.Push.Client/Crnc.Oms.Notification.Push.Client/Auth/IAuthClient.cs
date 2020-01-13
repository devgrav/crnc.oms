using System.Threading;
using System.Threading.Tasks;

namespace Crnc.Oms.Notification.Push.Client.Auth
{
    public interface IAuthClient
    {
        Task<AuthUserDto> GetJwtTokenAsync(AuthInfoDto authInfo, CancellationToken cancellationToken = default);
    }
}