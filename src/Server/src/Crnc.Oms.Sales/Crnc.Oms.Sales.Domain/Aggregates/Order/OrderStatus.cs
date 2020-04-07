using System.Collections.Generic;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    /// <summary>
    /// Status of order
    /// </summary>
    public class OrderStatus : Enumeration
    {
        public static readonly OrderStatus NotSent 
            = new OrderStatus(1, "Not sent");
        public static readonly OrderStatus NeedSignoff 
            = new OrderStatus(2, "Need signoff");
        public static readonly OrderStatus Signed 
            = new OrderStatus(3, "Signed");
        public static readonly OrderStatus ConvertedToJob 
            = new OrderStatus(4, "Converted to job");
        public static readonly OrderStatus Closed 
            = new OrderStatus(5, "Closed");

        private static readonly Dictionary<OrderStatus, List<OrderStatus>> AvailableOrderStatuses =
            new Dictionary<OrderStatus, List<OrderStatus>>
            {
                {
                    NotSent, new List<OrderStatus>
                    {
                        NotSent,NeedSignoff, Closed
                    }
                },
                {
                    NeedSignoff, new List<OrderStatus>
                    {
                        NeedSignoff,Signed, Closed
                    }
                },
                {
                    Signed, new List<OrderStatus>
                    {
                        Signed,ConvertedToJob, Closed
                    }
                },
                {
                    ConvertedToJob, new List<OrderStatus>
                    {
                        ConvertedToJob
                    }
                },
                {
                    Closed, new List<OrderStatus>
                    {
                        Closed
                    }              
                }
            };
        
        public OrderStatus() { }
        
        private OrderStatus(int value, string displayName) : base(value, displayName) { }

        public List<OrderStatus> GetAvailableStatuses()
        {
            return AvailableOrderStatuses[this];
        }
    }
}
