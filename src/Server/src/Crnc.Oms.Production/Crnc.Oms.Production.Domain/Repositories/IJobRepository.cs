using System;
using System.Threading.Tasks;
using Crnc.Oms.Production.Domain.SeedWork;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;


namespace Crnc.Oms.Production.Domain.Repositories
{
    public interface IJobRepository
        : IRepository<Job>
    {
        Task<Job> GetJobByOrderIdAsync(Guid orderId);
    }
}