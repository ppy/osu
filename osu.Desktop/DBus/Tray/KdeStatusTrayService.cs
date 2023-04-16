using System;
using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services.Kde;
using Tmds.DBus;

#nullable disable

namespace osu.Desktop.DBus.Tray
{
    /// <summary>
    ///     WIP
    /// </summary>
    public class TrayIconService : IMDBusObject, IStatusNotifierItem
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/StatusNotifierItem");

        public string CustomRegisterName => "org.kde.StatusNotifierItem.mfosu";
        public bool IsService => true;

        public Action WindowRaise { get; set; }

        public readonly StatusNotifierProperties KdeProperties = new StatusNotifierProperties
        {
            Menu = CanonicalTrayService.PATH,
            ItemIsMenu = true
        };

        public Task<object> GetAsync(string prop) => Task.FromResult(KdeProperties.Get(prop));

        public Task SetAsync(string prop, object val)
        {
            return Task.FromException(new InvalidOperationException("暂时不能修改"));
        }

        internal bool Set(string prop, object value)
        {
            bool result = KdeProperties.Set(prop, value);

            if (result)
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(prop, value));

            return result;
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
        }

        #region KDE DBus

        Task<StatusNotifierProperties> IStatusNotifierItem.GetAllAsync()
            => Task.FromResult(KdeProperties);

        public Task ContextMenuAsync(int x, int y) => Task.CompletedTask;

        public Task ActivateAsync(int x, int y)
        {
            WindowRaise?.Invoke();
            return Task.CompletedTask;
        }

        public Task SecondaryActivateAsync(int x, int y)
        {
            WindowRaise?.Invoke();
            return Task.CompletedTask;
        }

        public Task ScrollAsync(int delta, string orientation)
        {
            return Task.CompletedTask;
        }

        public event Action OnTitleChanged;

        public Task<IDisposable> WatchNewTitleAsync(Action handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnTitleChanged), handler);
        }

        public event Action OnIconChanged;

        public Task<IDisposable> WatchNewIconAsync(Action handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnIconChanged), handler);
        }

        public event Action OnNewAttentionIcon;

        public Task<IDisposable> WatchNewAttentionIconAsync(Action handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnNewAttentionIcon), handler);
        }

        public event Action OnOverlayIconChanged;

        public Task<IDisposable> WatchNewOverlayIconAsync(Action handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnOverlayIconChanged), handler);
        }

        public event Action OnMenuCreated;

        public Task<IDisposable> WatchNewMenuAsync(Action handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnMenuCreated), handler);
        }

        public event Action OnTooltipChanged;

        public Task<IDisposable> WatchNewToolTipAsync(Action handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnTooltipChanged), handler);
        }

        public event Action<string> OnStatusChanged;

        public Task<IDisposable> WatchNewStatusAsync(Action<string> handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnStatusChanged), handler);
        }

        public Task<string> GetCategoryAsync()
            => Task.FromResult(KdeProperties.Category);

        public Task<string> GetIdAsync()
            => Task.FromResult(KdeProperties.Id);

        public Task<string> GetTitleAsync()
            => Task.FromResult(KdeProperties.Title);

        public Task<string> GetStatusAsync()
            => Task.FromResult(KdeProperties.Status);

        public Task<int> GetWindowIdAsync()
            => Task.FromResult(KdeProperties.WindowId);

        public Task<string> GetIconThemePathAsync()
            => Task.FromResult(KdeProperties.IconThemePath);

        public Task<ObjectPath> GetMenuAsync()
            => Task.FromResult(CanonicalTrayService.PATH);

        public Task<bool> GetItemIsMenuAsync()
            => Task.FromResult(KdeProperties.ItemIsMenu);

        public Task<string> GetIconNameAsync()
            => Task.FromResult(KdeProperties.IconName);

        public Task<(int, int, byte[])[]> GetIconPixmapAsync()
            => Task.FromResult(KdeProperties.IconPixmap);

        public Task<string> GetOverlayIconNameAsync()
            => Task.FromResult(KdeProperties.OverlayIconName);

        public Task<(int, int, byte[])[]> GetOverlayIconPixmapAsync()
            => Task.FromResult(KdeProperties.OverlayIconPixmap);

        public Task<string> GetAttentionIconNameAsync()
            => Task.FromResult(KdeProperties.AttentionIconName);

        public Task<(int, int, byte[])[]> GetAttentionIconPixmapAsync()
            => Task.FromResult(KdeProperties.AttentionIconPixmap);

        public Task<string> GetAttentionMovieNameAsync()
            => Task.FromResult(KdeProperties.AttentionMovieName);

        public Task<(string, (int, int, byte[])[], string, string)> GetToolTipAsync()
            => Task.FromResult(KdeProperties.ToolTip);

        #endregion
    }
}
