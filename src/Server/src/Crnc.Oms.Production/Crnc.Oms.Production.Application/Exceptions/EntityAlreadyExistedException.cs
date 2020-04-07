using System;

namespace Crnc.Oms.Production.Application.Exceptions
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