using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace osu.Desktop.DBus.Tray
{
    /// <summary>
    ///     WIP
    /// </summary>
    public class KdeStatusTrayService : IKdeStatusNotifierItem
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/StatusNotifierItem");

        public Action WindowRaise { get; set; }

        private readonly StatusNotifierItemProperties kdeProperties = new StatusNotifierItemProperties();

        public Task<object> GetAsync(string prop) => Task.FromResult(kdeProperties.Get(prop));

        public Task SetAsync(string prop, object val)
        {
            throw new InvalidOperationException("暂时不能修改");
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
        }

        #region KDE DBus

        Task<StatusNotifierItemProperties> IKdeStatusNotifierItem.GetAllAsync()
            => Task.FromResult(kdeProperties);

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
            => Task.FromResult(kdeProperties.Category);

        public Task<string> GetIdAsync()
            => Task.FromResult(kdeProperties.Id);

        public Task<string> GetTitleAsync()
            => Task.FromResult(kdeProperties.Title);

        public Task<string> GetStatusAsync()
            => Task.FromResult(kdeProperties.Status);

        public Task<int> GetWindowIdAsync()
            => Task.FromResult(kdeProperties.WindowId);

        public Task<string> GetIconThemePathAsync()
            => Task.FromResult(kdeProperties.IconThemePath);

        public Task<ObjectPath> GetMenuAsync()
            => Task.FromResult(CanonicalTrayService.PATH);

        public Task<bool> GetItemIsMenuAsync()
            => Task.FromResult(kdeProperties.ItemIsMenu);

        public Task<string> GetIconNameAsync()
            => Task.FromResult(kdeProperties.IconName);

        public Task<(int, int, byte[])[]> GetIconPixmapAsync()
            => Task.FromResult(kdeProperties.IconPixmap);

        public Task<string> GetOverlayIconNameAsync()
            => Task.FromResult(kdeProperties.OverlayIconName);

        public Task<(int, int, byte[])[]> GetOverlayIconPixmapAsync()
            => Task.FromResult(kdeProperties.OverlayIconPixmap);

        public Task<string> GetAttentionIconNameAsync()
            => Task.FromResult(kdeProperties.AttentionIconName);

        public Task<(int, int, byte[])[]> GetAttentionIconPixmapAsync()
            => Task.FromResult(kdeProperties.AttentionIconPixmap);

        public Task<string> GetAttentionMovieNameAsync()
            => Task.FromResult(kdeProperties.AttentionMovieName);

        public Task<(string, (int, int, byte[])[], string, string)> GetToolTipAsync()
            => Task.FromResult(kdeProperties.ToolTip);

        #endregion
    }
}
