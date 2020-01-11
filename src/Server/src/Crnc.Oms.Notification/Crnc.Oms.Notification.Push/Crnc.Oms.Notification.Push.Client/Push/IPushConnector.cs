using System;
using System.Threading.Tasks;

namespace Crnc.Oms.Notification.Push.Client.Push
{
    public interface IPushConnector
        : IDisposable
    { 
        Task Connect();
        
        Task Disconnect();
    }
}