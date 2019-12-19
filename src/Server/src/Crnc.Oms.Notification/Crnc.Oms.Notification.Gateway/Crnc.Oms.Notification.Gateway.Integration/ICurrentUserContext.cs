namespace Crnc.Oms.Notification.Gateway.Integration
{
    public interface ICurrentUserContext
    {
        string AuthToken { get; }
    }
}