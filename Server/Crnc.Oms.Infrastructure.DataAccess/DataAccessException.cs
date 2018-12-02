using System;

namespace Crnc.Oms.Infrastructure.DataAccess
{
    public class DataAccessException
        : ApplicationException
    {
        public DataAccessException(string message)
            : base(message)
        {
            
        }

        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}