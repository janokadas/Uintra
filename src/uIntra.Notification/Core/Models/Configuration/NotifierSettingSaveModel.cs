using System;
using uIntra.Core.TypeProviders;

namespace uIntra.Notification
{
    public class NotifierSettingModel<T>
        where T : INotifierTemplate
    {
        public IIntranetType ActivityType { get; set; }
        public Enum NotificationType { get; set; }
        public Enum NotifierType { get; set; }
        public bool IsEnabled { get; set; }
        public string NotificationInfo { get; set; } 
        public T Template { get; set; }
    }
}