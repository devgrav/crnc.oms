using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Output
{
    public class GetNewOrderOutputDto
    {
        public int JobType { get; set; }
        
        public List<TextValueOutputDto<int, string>> JobTypes { get; set; }
        public string JobDescription { get; set; }
        
        public string CustomerContactPersonFirstName { get; set; }
        
        public string CustomerContactPersonMiddleName { get; set; }
        
        public string CustomerContactPersonLastName { get; set; }
        
        public string CustomerTitle { get; set; }
        
        public string CustomerAbbreviation { get; set; }
        
        public string CustomerContactPersonEmail { get; set; }
        
        public string CustomerContactPersonPhone { get; set; }
    }
}