using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace M.DBus.Services.Notifications
{
    [DBusInterface("org.freedesktop.Notifications")]
    public interface INotifications : IDBusObject
    {
        Task<uint> NotifyAsync(string appName, uint replacesID, string appIcon, string summary, string details, string[] actions, IDictionary<string, object> hints, int displayTime);
        Task CloseNotificationAsync(uint id);
        Task<string[]> GetCapabilitiesAsync();
        Task<(string name, string vendor, string version, string specVersion)> GetServerInformationAsync();

        Task<IDisposable> WatchNotificationClosedAsync(Action<(uint arg0, uint arg1)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchActionInvokedAsync(Action<(uint arg0, string arg1)> handler, Action<Exception> onError = null);
    }
}
