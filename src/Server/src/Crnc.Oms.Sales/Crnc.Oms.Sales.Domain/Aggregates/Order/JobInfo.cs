using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    public class JobInfo
        : IValueObject
    {
        /// <summary>
        /// Job type
        /// </summary>
        public JobType Type { get; private set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string Description { get; private set; }
        
        /// <summary>
        /// Job id
        /// </summary>
        public Guid? Id { get; private set; }

        /// <summary>
        /// Job number
        /// </summary>
        public string Number { get; private set; }

        public JobInfo(JobType type, string description)
        {
            if(string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            
            Type = type;
            Description = description;
        }
        
        public JobInfo(JobType type, string description, Guid id, string number)
        {
            Type = type;
            Description = description;
            Id = id;
            Number = number;
        }
        
        protected JobInfo()
        {
            
        }
    }
}