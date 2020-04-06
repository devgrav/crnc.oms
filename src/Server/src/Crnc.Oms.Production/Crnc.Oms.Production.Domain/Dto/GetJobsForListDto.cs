using System.Collections.Generic;
using System.ComponentModel;

namespace Crnc.Oms.Production.Domain.Dto
{
    public class GetJobsForListDto
    {
        public List<JobListItemDto> Items { get; set; }
    }
}