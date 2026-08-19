using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Crnc.Oms.Production.Domain.Repositories;
using Crnc.Oms.Production.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Crnc.Oms.Production.DataAccess.Repositories
{
    public class JobRepository
        : Repository<Job>, IJobRepository
    {
        public JobRepository(ProductionDataContext dbContext)
            :base(dbContext)
        {
        }

        public async Task<Job> GetJobByOrderIdAsync(Guid orderId)
        {
            return  await _dataContext.Jobs.SingleOrDefaultAsync(x => x.OrderId == orderId);
        }
    }
}