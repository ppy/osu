using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using M.DBus;
using Tmds.DBus;

namespace osu.Desktop.DBus.Tray
{
    [DBusInterface("com.canonical.dbusmenu")]
    public interface ICanonicalDBusMenu : IDBusObject
    {
        Task<(uint revision, (int, IDictionary<string, object>, object[]) layout)> GetLayoutAsync(int parentId, int recursionDepth, string[] propertyNames);
        Task<(int, IDictionary<string, object>)[]> GetGroupPropertiesAsync(int[] ids, string[] propertyNames);
        Task<object> GetPropertyAsync(int id, string name);
        Task EventAsync(int id, string eventId, object data, uint timestamp);
        Task<int[]> EventGroupAsync((int, string, object, uint)[] events);
        Task<bool> AboutToShowAsync(int id);
        Task<(int[] updatesNeeded, int[] idErrors)> AboutToShowGroupAsync(int[] ids);
        Task<IDisposable> WatchItemsPropertiesUpdatedAsync(Action<((int, IDictionary<string, object>)[] updatedProps, (int, string[])[] removedProps)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchLayoutUpdatedAsync(Action<(uint revision, int parent)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchItemActivationRequestedAsync(Action<(int id, uint timestamp)> handler, Action<Exception> onError = null);
        Task<object> GetAsync(string prop);
        Task<CanonicalDBusMenuProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);

        //Extensions
        Task<uint> GetVersionAsync();
        Task<string> GetTextDirectionAsync();
        Task<string> GetStatusAsync();
        Task<string[]> GetIconThemePathAsync();
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class CanonicalDBusMenuProperties
    {
        public uint Version
        {
            get => _Version;

            set => _Version = value;
        }

        public string TextDirection
        {
            get => _TextDirection;

            set => _TextDirection = value;
        }

        public string Status
        {
            get => _Status;

            set => _Status = value;
        }

        public string[] IconThemePath
        {
            get => _IconThemePath;

            set => _IconThemePath = value;
        }

        private uint _Version = 4;

        private string _TextDirection = "ltr";

        private string _Status = "normal";

        private string[] _IconThemePath = Array.Empty<string>();

        private IDictionary<string, object> members;

        public object Get(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.GetValueFor(this, prop, members);
        }

        internal bool Set(string name, object newValue)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.SetValueFor(this, name, newValue, members);
        }

        internal bool Contains(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.CheckifContained(this, prop, members);
        }
    }
}
