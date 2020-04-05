using System;
using System.Collections.Generic;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Domain.Aggregates.Order;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Output
{
    public class GetOrderOutputDto
    {
        public Guid Id { get; set; }
        public OrderStatusEnum Status { get; set; }
        public List<TextValueOutputDto<int, string>> Statuses { get; set; }
        public string DateCreated { get; set; }
        public JobType JobType { get; set; }
        public List<TextValueOutputDto<int, string>> JobTypes { get; set; }
        
        public MaterialSource? MaterialSource { get; set; }
        
        public List<TextValueOutputDto<int, string>> MaterialSources { get; set; }
        
        public SignoffType? SignoffType { get; set; }
        
        public List<TextValueOutputDto<int, string>> SignoffTypes { get; set; }
        public string JobDescription { get; set; }
        public string CustomerContactPersonFirstName { get; set; }
        public string CustomerContactPersonLastName { get; set; }
        public string CustomerTitle { get; set; }
        public string CustomerAbbreviation { get; set; }
        public string CustomerContactPersonEmail { get; set; }
        public string CustomerContactPersonPhone { get; set; }
        public string DateSentToCustomer { get; set; }
    }
}