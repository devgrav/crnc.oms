using System;

namespace Crnc.Oms.Sales.Domain.Dto
{
    public class NotifyUserInputDto
    {
        public Guid UserId { get; set; }
        
        public string Message { get; set; }
    }
}