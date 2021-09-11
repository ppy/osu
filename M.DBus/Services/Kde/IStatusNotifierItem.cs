using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace M.DBus.Services.Kde
{
    [DBusInterface("org.kde.StatusNotifierItem")]
    public interface IStatusNotifierItem : IDBusObject
    {
        Task ContextMenuAsync(int x, int y);
        Task ActivateAsync(int x, int y);
        Task SecondaryActivateAsync(int x, int y);
        Task ScrollAsync(int delta, string orientation);
        Task<IDisposable> WatchNewTitleAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewAttentionIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewOverlayIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewMenuAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewToolTipAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewStatusAsync(Action<string> handler, Action<Exception> onError = null);
        Task<object> GetAsync(string prop);
        Task<StatusNotifierProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);

        //Extensions
        Task<string> GetCategoryAsync();
        Task<string> GetIdAsync();
        Task<string> GetTitleAsync();
        Task<string> GetStatusAsync();
        Task<int> GetWindowIdAsync();
        Task<string> GetIconThemePathAsync();
        Task<ObjectPath> GetMenuAsync();
        Task<bool> GetItemIsMenuAsync();
        Task<string> GetIconNameAsync();
        Task<(int, int, byte[])[]> GetIconPixmapAsync();
        Task<string> GetOverlayIconNameAsync();
        Task<(int, int, byte[])[]> GetOverlayIconPixmapAsync();
        Task<string> GetAttentionIconNameAsync();
        Task<(int, int, byte[])[]> GetAttentionIconPixmapAsync();
        Task<string> GetAttentionMovieNameAsync();
        Task<(string, (int, int, byte[])[], string, string)> GetToolTipAsync();
    }
}
