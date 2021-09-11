using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace M.DBus.Services.Kde
{
    [DBusInterface("org.kde.StatusNotifierWatcher")]
    public interface IStatusNotifierWatcher : IDBusObject
    {
        Task RegisterStatusNotifierItemAsync(string service);
        Task RegisterStatusNotifierHostAsync(string service);
        Task<IDisposable> WatchStatusNotifierItemRegisteredAsync(Action<string> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchStatusNotifierItemUnregisteredAsync(Action<string> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchStatusNotifierHostRegisteredAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchStatusNotifierHostUnregisteredAsync(Action handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<StatusNotifierWatcherProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

}
