using System;
using Crnc.Oms.Sales.Domain.SeedWork;

namespace Crnc.Oms.Sales.Domain.Aggregates.Order
{
    /// <summary>
    /// Order for job
    /// </summary>
    public class Order
        : DomainEntity, IAggregateRoot
    {
        /// <summary>
        /// Number of order, tempalte E-value of id
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Date of created
        /// </summary>
        public DateTime DateCreated { get; private set; }

        /// <summary>
        /// Date sent to customer
        /// </summary>
        public DateTime? DateSentToCustomer { get; private set; }

        /// <summary>
        /// Job type
        /// </summary>
        public JobType JobType { get; private set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string JobDescription { get; private set; }

        /// <summary>
        /// Current status of order
        /// </summary>
        public OrderStatus Status { get; private set; }
        
        /// <summary>
        /// Date of change status of order
        /// </summary>
        public DateTime StatusDate { get; private set; }

        /// <summary>
        /// Source of material
        /// </summary>
        public MaterialSource? MaterialSource { get; private set; }

        /// <summary>
        /// Type of signoff
        /// </summary>
        public SignoffType? SignOffType { get; private set; }

        /// <summary>
        /// Customer of order
        /// </summary>
        public Customer Customer { get; private set; }

        public Order(Guid id, DateTime dateCreated, JobType jobType, string jobDescription, Customer customer)
            : base(id)
        {
            if(string.IsNullOrWhiteSpace(jobDescription))
                throw new ArgumentNullException(nameof(jobDescription));

            Customer = customer ?? throw new ArgumentNullException(nameof(customer));
            DateCreated = dateCreated;
            JobType = jobType;
            JobDescription = jobDescription;
            Status = OrderStatus.NotSent;
            StatusDate = dateCreated;
            Number = Id.ToString(); //TODO: add different algoritm
        }

        public void Edit(JobType jobType, string jobDescription, MaterialSource? materialSource,
            SignoffType? signoffType, Customer customer)
        {
            JobType = jobType;
            JobDescription = jobDescription;
            MaterialSource = materialSource;
            SignOffType = signoffType;
            Customer = customer;
        }
        
        public void ChangeStatus(OrderStatus status, DateTime date)
        {
            var oldStatus = Status;
            
            Status = status;
            StatusDate = date;
            
            AddDomainEvent(new StatusChanged(oldStatus, Status));
        }

        protected Order()
        {
            
        }
    }
}
