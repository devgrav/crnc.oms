using System;

namespace Crnc.Oms.Sales.Application.Exceptions
{
    public class EntityAlreadyExistedException
        : ApplicationException
    {
        public EntityAlreadyExistedException(string message = "Entity has already existed")
            : base(message)
        {

        }
    }
}