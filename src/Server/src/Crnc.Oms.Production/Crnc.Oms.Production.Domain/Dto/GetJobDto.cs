using System;
using System.Collections.Generic;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;

namespace Crnc.Oms.Production.Domain.Dto
{
    public class GetJobDto
    {
        public Guid Id { get; set; }
        
        public string Number { get; set; }
        
        public string DateCreated { get; set; }
        
        public string JobType { get; set; }
        
        public string JobDescription { get; set; }
        
        public bool IsJobCompeted { get; set; }
        
        public string MaterialSource { get; set; }
        
        public string Manager { get; set; }
        
        public Priority PriorityEnum { get; set; }
        
        public List<TextValueDto<int, string>> Priorities { get; set; }
    }
}