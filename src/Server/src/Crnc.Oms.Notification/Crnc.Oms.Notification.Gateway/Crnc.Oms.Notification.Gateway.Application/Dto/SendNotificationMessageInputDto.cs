using System;

namespace Crnc.Oms.Notification.Gateway.Application.Dto
{
    public class SendNotificationMessageInputDto
    {
        public string Receiver { get; set; }

        public string Message { get; set; }

        public ChannelType Channel { get; set; }
    }
}