using System.Threading;
using System.Threading.Tasks;

namespace Crnc.Oms.Sales.Application
{
    /// <summary>
    /// Handler of command for user scenario
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public interface IUseCaseCommandHandler<TIn>

    {
        /// <summary>
        /// Handle command async
        /// </summary>
        /// <param name="command">Data of command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task HandleAsync(TIn command, CancellationToken cancellationToken=default); 
    }
}