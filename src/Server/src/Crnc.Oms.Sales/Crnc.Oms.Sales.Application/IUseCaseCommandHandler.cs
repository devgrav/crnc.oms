using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Crnc.Oms.Sales.Application
{
    /// <summary>
    /// Handler of command for user scenario
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public interface IUseCaseCommandHandler<TIn, TOut>
        : IRequestHandler<TIn, TOut>
        where TIn: IUseCaseCommand<TOut>

    {
 
    }
}