#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services;
using M.DBus.Services.Notifications;
using M.DBus.Utils;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    public class SystemNotificationManager : IHandleSystemNotifications
    {
        private DBusMgrNew dBusManager;

        public void SetDBusManager(DBusMgrNew dBusManager)
        {
            this.dBusManager = dBusManager;
        }

        #region 通知

        internal void OnEnableNotificationsChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
                connectToNotifications();
            else
                systemNotification = null;
        }

        private INotifications systemNotification;

        private bool notificationWatched;
        private readonly Dictionary<uint, SystemNotification> notifications = new Dictionary<uint, SystemNotification>();

        private bool connectToNotifications()
        {
            try
            {
                var path = new ObjectPath("/org/freedesktop/Notifications");
                systemNotification = dBusManager.GetProxyObject<INotifications>(path, path.ToServiceName());

                if (!notificationWatched)
                {
                    //bug: 在gnome上会导致调用两次？
                    systemNotification.WatchActionInvokedAsync(onActionInvoked);
                    systemNotification.WatchNotificationClosedAsync(onNotificationClosed);
                    notificationWatched = true;
                }
            }
            catch (Exception e)
            {
                systemNotification = null;
                notificationWatched = false;
                Logger.Error(e, "未能连接到 org.freedesktop.Notifications, 请检查相关配置");
                return false;
            }

            return true;
        }

        private void onNotificationClosed((uint id, uint reason) singal)
        {
            SystemNotification notification;

            if (notifications.TryGetValue(singal.id, out notification))
            {
                notification.OnClosed?.Invoke(singal.reason.ToCloseReason());
                notifications.Remove(singal.id);
            }
        }

        private void onActionInvoked((uint id, string actionKey) obj)
        {
            SystemNotification notification;

            if (notifications.TryGetValue(obj.id, out notification))
            {
                notification.Actions.FirstOrDefault(a => a.Id == obj.actionKey)?.OnInvoked?.Invoke();
                notification.OnClosed?.Invoke(CloseReason.ActionInvoked);

                notifications.Remove(obj.id);
                Task.Run(async () => await CloseNotificationAsync(obj.id).ConfigureAwait(false));
            }
        }

        public async Task<uint> PostAsync(SystemNotification notification)
        {
            try
            {
                if (systemNotification != null)
                {
                    var target = notification.ToDBusObject();

                    uint result = await systemNotification.NotifyAsync(target.appName,
                        target.replacesID,
                        target.appIcon,
                        target.title,
                        target.description,
                        target.actions,
                        target.hints,
                        target.displayTime).ConfigureAwait(false);

                    notifications[result] = notification;
                    return result;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "发送系统通知时出现了问题");
            }

            return 0;
        }

        public async Task<bool> CloseNotificationAsync(uint id)
        {
            if (systemNotification != null)
            {
                await systemNotification.CloseNotificationAsync(id).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public async Task<string[]> GetCapabilitiesAsync()
        {
            if (systemNotification != null)
            {
                return await systemNotification.GetCapabilitiesAsync().ConfigureAwait(false);
            }

            return Array.Empty<string>();
        }

        private readonly (string name, string vendor, string version, string specVersion) defaultServerInfo = ("mfosu", "mfosu", "0", "0");

        public async Task<(string name, string vendor, string version, string specVersion)> GetServerInformationAsync()
        {
            if (systemNotification != null)
            {
                return await systemNotification.GetServerInformationAsync().ConfigureAwait(false);
            }

            return defaultServerInfo;
        }

        #endregion
    }
}
