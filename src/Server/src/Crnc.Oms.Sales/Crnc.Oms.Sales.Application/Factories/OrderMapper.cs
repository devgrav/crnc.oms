using System;
using Crnc.Oms.Sales.Application.Features.Orders.Dto.Input;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Application.Factories
{
    public static class OrderMapper
    {
        public static Order MapNewOrder(CreateOrderInputDto dto, ICurrentDateTimeProvider currentDateTimeProvider)
        {
            var customer = new Customer(
                new Title(dto.CustomerTitle, 
                    new NameAbbreviation(dto.CustomerAbbreviation)), 
                new ContactPerson(new FullName(
                        dto.CustomerContactPersonFirstName,
                        dto.CustomerContactPersonLastName), 
                    new Email(dto.CustomerContactPersonEmail),
                    new Phone(dto.CustomerContactPersonPhone)));

            var order = new Order(
                Guid.NewGuid(), 
                currentDateTimeProvider.GetNow(), 
                dto.JobType, 
                dto.JobDescription,
                customer);

            return order;
        }
        
        public static Order MapExistedOrder(Order order, EditOrderInputDto dto)
        {
            var customer = new Customer(
                new Title(dto.CustomerTitle, 
                    new NameAbbreviation(dto.CustomerAbbreviation)), 
                new ContactPerson(new FullName(
                        dto.CustomerContactPersonFirstName,
                        dto.CustomerContactPersonLastName), 
                    new Email(dto.CustomerContactPersonEmail),
                    new Phone(dto.CustomerContactPersonPhone)));

            order.Edit(dto.JobType, 
                dto.JobDescription, 
                dto.MaterialSource, 
                dto.SignOffType, 
                customer);

            return order;
        }
        
    }
}