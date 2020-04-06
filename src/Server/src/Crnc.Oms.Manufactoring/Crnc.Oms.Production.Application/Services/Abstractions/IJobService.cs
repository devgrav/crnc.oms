using System;
using System.Threading.Tasks;
using Crnc.Oms.Production.Domain.Dto;

namespace Crnc.Oms.Production.Application.Services
{
    public interface IJobService
    {
        Task<JobDto> GetJob(Guid id);
    }
}