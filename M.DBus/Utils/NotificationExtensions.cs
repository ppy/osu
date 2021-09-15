using System.Collections.Generic;
using M.DBus.Services.Notifications;

namespace M.DBus.Utils
{
    public static class NotificationExtensions
    {
        public static (string appName,
            uint replacesID,
            string appIcon,
            string title,
            string description,
            string[] actions,
            IDictionary<string, object> hints,
            int displayTime) ToDBusObject(this SystemNotification notification)
        {
            return (
                notification.Name,
                0,
                notification.IconName,
                notification.Title,
                notification.Description,
                notification.Actions,
                notification.Hints,
                notification.DisplayTime);
        }
    }
}
