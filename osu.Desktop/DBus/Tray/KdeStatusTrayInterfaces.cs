//为什么就没有一个统一点的托盘标准呢...

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using M.DBus;
using Tmds.DBus;

namespace osu.Desktop.DBus.Tray
{
    [DBusInterface("org.kde.StatusNotifierItem")]
    public interface IKdeStatusNotifierItem : IDBusObject
    {
        Task ContextMenuAsync(int X, int Y);
        Task ActivateAsync(int X, int Y);
        Task SecondaryActivateAsync(int X, int Y);
        Task ScrollAsync(int Delta, string Orientation);
        Task<IDisposable> WatchNewTitleAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewAttentionIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewOverlayIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewMenuAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewToolTipAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewStatusAsync(Action<string> handler, Action<Exception> onError = null);
        Task<object> GetAsync(string prop);
        Task<StatusNotifierItemProperties> GetAllAsync();
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

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class StatusNotifierItemProperties
    {
        public string Category
        {
            get => _Category;

            set => _Category = value;
        }

        public string Id
        {
            get => _Id;

            set => _Id = value;
        }

        public string Title
        {
            get => _Title;

            set => _Title = value;
        }

        public string Status
        {
            get => _Status;

            set => _Status = value;
        }

        public int WindowId
        {
            get => _WindowId;

            set => _WindowId = value;
        }

        public string IconThemePath
        {
            get => _IconThemePath;

            set => _IconThemePath = value;
        }

        public ObjectPath Menu
        {
            get => _Menu;

            set => _Menu = value;
        }

        public bool ItemIsMenu
        {
            get => _ItemIsMenu;

            set => _ItemIsMenu = value;
        }

        public string IconName
        {
            get => _IconName;

            set => _IconName = value;
        }

        public (int, int, byte[])[] IconPixmap
        {
            get => _IconPixmap;

            set => _IconPixmap = value;
        }

        public string OverlayIconName
        {
            get => _OverlayIconName;

            set => _OverlayIconName = value;
        }

        public (int, int, byte[])[] OverlayIconPixmap
        {
            get => _OverlayIconPixmap;

            set => _OverlayIconPixmap = value;
        }

        public string AttentionIconName
        {
            get => _AttentionIconName;

            set => _AttentionIconName = value;
        }

        public (int, int, byte[])[] AttentionIconPixmap
        {
            get => _AttentionIconPixmap;

            set => _AttentionIconPixmap = value;
        }

        public string AttentionMovieName
        {
            get => _AttentionMovieName;

            set => _AttentionMovieName = value;
        }

        public (string, (int, int, byte[])[], string, string) ToolTip
        {
            get => _ToolTip;

            set => _ToolTip = value;
        }

        private string _Category;

        private string _Id = "mfosu";

        private string _Title;

        private string _Status;

        private int _WindowId;

        private string _IconThemePath;

        private ObjectPath _Menu;

        private bool _ItemIsMenu;

        private string _IconName;

        private (int, int, byte[])[] _IconPixmap;

        private string _OverlayIconName;

        private (int, int, byte[])[] _OverlayIconPixmap;

        private string _AttentionIconName;

        private (int, int, byte[])[] _AttentionIconPixmap;

        private string _AttentionMovieName;

        private (string, (int, int, byte[])[], string, string) _ToolTip;

        private readonly IDictionary<string, object> members;

        public StatusNotifierItemProperties()
        {
            members = ServiceUtils.GetMembers(this);
        }

        public object Get(string prop) => ServiceUtils.GetValueFor(this, prop, members);

        internal bool Set(string name, object newValue) => ServiceUtils.SetValueFor(this, name, newValue, members);
    }
}
