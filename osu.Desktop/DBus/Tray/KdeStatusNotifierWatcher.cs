using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Tmds.DBus;

namespace osu.Desktop.DBus.Tray
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

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class StatusNotifierWatcherProperties
    {
        public string[] RegisteredStatusNotifierItems
        {
            get => _RegisteredStatusNotifierItems;

            set => _RegisteredStatusNotifierItems = value;
        }

        public bool IsStatusNotifierHostRegistered
        {
            get => _IsStatusNotifierHostRegistered;

            set => _IsStatusNotifierHostRegistered = value;
        }

        public int ProtocolVersion
        {
            get => _ProtocolVersion;

            set => _ProtocolVersion = value;
        }

        private string[] _RegisteredStatusNotifierItems;

        private bool _IsStatusNotifierHostRegistered;

        private int _ProtocolVersion;
    }
}
