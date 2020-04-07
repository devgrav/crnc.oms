using System;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;

namespace Crnc.Oms.Production.Domain.Dto
{
    public class CreateJobDto
    {
        public JobType JobType { get; set; }
        
        public string JobDescription { get; set; }

        public MaterialSource MaterialSource { get; set; }
        
        public string ManagerFullName { get; set; }
        
        public string ManagerLogin { get; set; }

        public Guid OrderId { get; set; }

        public string OrderNumber { get; set; }
    }
}