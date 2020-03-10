namespace Crnc.Oms.Notification.Gateway.Integration.Settings
{
    public class IntegrationEndpointSettings
    {
        public string EmailNotificationServiceEndpoint { get; set; }
        public string PushNotificationServiceEndpoint { get; set; }
        public string SecurityServiceEndpoint { get; set; }
        public string MessageBrokerEndpoint { get; set; }
    }
}