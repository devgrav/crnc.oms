using System;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Output;

namespace Crnc.Oms.Sales.Application.Features.Orders.Dto.Input
{
    public class SetJobInfoInputDto
        : IUseCaseCommand<EmptyOutputDto>
    {
        public Guid JobId { get; set; }

        public string JobNumber { get; set; }
    }
}