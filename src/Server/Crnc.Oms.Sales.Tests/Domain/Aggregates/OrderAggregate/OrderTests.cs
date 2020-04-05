using System;
using System.Collections.Generic;
using System.Linq;
using Crnc.Oms.Sales.Domain.Aggregates.Order;
using FluentAssertions;
using Xunit;

namespace Crnc.Oms.Sales.Tests.Domain.Aggregates.OrderAggregate
{
    public class OrderTests
    {
        [Fact]
        public void ComposeOrderNumber_WithGuidId_ExpectedResult()
        {
            var list = new List<int>
            {
                1, 2, 3, 4, 5
            };

            list.Where(x => x == 1);
            
            //Arrange
            var id = Guid.Parse("3ed0adf9-7ff4-4b09-ae2f-db41e293d8c8");
            var expected = "3ed0adf9";
            var order = new Order(id,
                DateTime.Now,
                JobType.New,
                "Some job",
                new Customer(new Title("Some title", new NameAbbreviation("ST")),
                    new ContactPerson(
                        new FullName("John", "Galt"),
                        new Email("john_galt@crnc.ru"),
                        new Phone("+79122341212"))),
                new Manager(
                    new FullName("Terry", "Pratchet"), 
                    new Email("some_mail@crnc.ru"), 
                    "terry_pratchet",  
                    Guid.NewGuid()));
            
            
            //Act
            var actual = order.ComposeOrderNumber();
            
            //Assert
            actual.Should().Be(expected);
        }
    }
}