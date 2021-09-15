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
            List<string> actions = new List<string>();

            foreach (var notificationAction in notification.Actions)
            {
                actions.AddRange(new[]
                {
                    notificationAction.Id,
                    notificationAction.Description
                });
            }

            return (
                notification.Name,
                0,
                notification.IconName,
                notification.Title,
                notification.Description,
                actions.ToArray(),
                notification.Hints,
                notification.DisplayTime);
        }

        public static CloseReason ToCloseReason(this uint reason)
        {
            switch (reason)
            {
                case 1:
                    return CloseReason.Expired;

                case 2:
                    return CloseReason.ClosedByUser;

                case 3:
                    return CloseReason.ClosedByCall;

                default:
                    return CloseReason.Undefined;
            }
        }
    }

    public enum CloseReason
    {
        Expired,
        ClosedByUser,
        ClosedByCall,
        ActionInvoked,
        Undefined
    }
}
