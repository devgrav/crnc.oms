using System;
using System.Collections.Generic;
using System.Linq;
using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
using Crnc.Oms.Production.Domain.Aggregates.Order;
using Crnc.Oms.Production.Domain.SeedWork;

namespace Crnc.Oms.Production.Domain.Aggregates.JobAggregate
{
    /// <summary>
    /// Job
    /// </summary>
    public class Job
        : DomainEntity, IAggregateRoot
    {
        /// <summary>
        /// Number of job
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Date of created
        /// </summary>
        public DateTime DateCreated { get; private set; }

        /// <summary>
        /// Job type
        /// </summary>
        public JobType JobType { get; private set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string JobDescription { get; private set; }

        /// <summary>
        /// Job finished
        /// </summary>
        public bool IsJobCompeted { get; private set; }

        /// <summary>
        /// Source of material
        /// </summary>
        public MaterialSource MaterialSource { get; private set; }

        /// <summary>
        /// Manager of manufactoring
        /// </summary>
        public Manager Manager { get; private set; }

        /// <summary>
        ///Job's priority
        /// </summary>
        public Priority Priority { get; private set; }

        /// <summary>
        /// Id of order
        /// </summary>
        public Guid OrderId { get; set; }
        
        /// <summary>
        /// Number of order
        /// </summary>
        public string OrderNumber { get; set; }

        public Job(Guid id, DateTime dateCreated, JobType jobType, string jobDescription, MaterialSource materialSource, Manager manager, Guid orderId, string orderNumber)
            : base(id)
        {
            if(string.IsNullOrWhiteSpace(jobDescription))
                throw new ArgumentNullException(nameof(jobDescription));
            
            if(manager == null)
                throw new ArgumentNullException(nameof(manager));
            
            if(string.IsNullOrWhiteSpace(orderNumber))
                throw new ArgumentNullException(nameof(orderNumber));
            
            DateCreated = dateCreated;
            JobType = jobType;
            JobDescription = jobDescription;
            Number = ComposeJobNumber();
            Manager = manager;
            Priority = Priority.Low;
            OrderId = orderId;
            OrderNumber = orderNumber;
            MaterialSource = materialSource;
        }

        public string ComposeJobNumber()
        {
            return Id.ToString().Split(new string[]{"-"}, StringSplitOptions.RemoveEmptyEntries)[0].ToLower();
        }

        public void FinishJob()
        {
            IsJobCompeted = true;
        }

        public void ChangePriority(Priority priority)
        {
            Priority = priority;
        }

        protected Job()
        {
            
        }
    }
}
