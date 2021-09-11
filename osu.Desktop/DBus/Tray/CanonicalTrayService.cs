using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M.DBus.Tray;
using Tmds.DBus;
using Logger = osu.Framework.Logging.Logger;

namespace osu.Desktop.DBus.Tray
{
    /// <summary>
    /// todo: 找到文档并实现所有目前未实现的功能
    /// </summary>
    public class CanonicalTrayService : ICanonicalDBusMenu
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/MenuBar");

        #region Canonical DBus

        private readonly CanonicalDBusMenuProperties canonicalProperties = new CanonicalDBusMenuProperties();

        private uint menuRevision;

        private readonly IDictionary<string, object> layout = new Dictionary<string, object>
        {
            ["children-display"] = "submenu"
        };

        #region 列表物件存储

        private readonly Dictionary<int, SimpleEntry> entries = new Dictionary<int, SimpleEntry>();
        private readonly List<object> cachedMenuEntries = new List<object>();

        private int lastEntryId = 1;

        private void addEntry(SimpleEntry entry)
        {
            entries[lastEntryId] = entry;
            lastEntryId++;
        }

        public void AddEntryRange(SimpleEntry[] entries)
        {
            foreach (var entry in entries)
            {
                addEntry(entry);
            }

            triggerLayoutUpdate();
        }

        public void AddEntryToMenu(SimpleEntry entry)
        {
            addEntry(entry);
            triggerLayoutUpdate();
        }

        public void RemoveEntryFromMenu(SimpleEntry entry)
        {
            var key = entries.FirstOrDefault(p => p.Value == entry);

            if (key.Value != null)
                entries.Remove(key.Key);
            else
                throw new InvalidOperationException($"给定的 {entry} 不在列表中");

            triggerLayoutUpdate();
        }

        private void triggerLayoutUpdate()
        {
            menuRevision++;
            OnLayoutUpdated?.Invoke((menuRevision, 0));
        }

        #endregion

        /// <summary>
        /// revision: 作用未知
        /// layout(int): 作用未知
        /// layout(IDictionary string, object ): layout类型
        /// object[]: 目录列表
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="recursionDepth"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        /// 你知道查不到文档被迫连蒙带猜有多痛苦么
        /// 我在这小东西上面花了整整2小时
        public Task<(uint revision, (int, IDictionary<string, object>, object[]) layout)> GetLayoutAsync(int parentId, int recursionDepth, string[] propertyNames)
        {
            if (cachedMenuEntries.Count != entries.Count)
            {
                Logger.Log("重新构建列表...");
                cachedMenuEntries.Clear();

                foreach (var entry in entries)
                {
                    //id, dict, subItems
                    Logger.Log($"id: {entry.Key}, value: {entry.Value}");
                    cachedMenuEntries.Add((entry.Key, TrayUtils.ToDictionary(entry.Value), Array.Empty<object>()));
                }
            }

            var result = (menuRevision, (0, layout, cachedMenuEntries.ToArray()));

            return Task.FromResult(result);
        }

        public Task<(int, IDictionary<string, object>)[]> GetGroupPropertiesAsync(int[] ids, string[] propertyNames)
        {
            Logger.Log($"未实现的方法被调用：GetGroupPropertiesAsync: ids: {ids} | propertyNames: {propertyNames}");
            throw new NotImplementedException();
        }

        public Task<object> GetPropertyAsync(int id, string name)
        {
            Logger.Log($"未实现的方法被调用：GetPropertyAsync: id: {id} | name: {name}");
            throw new NotImplementedException();
        }

        public Task EventAsync(int id, string eventId, object data, uint timestamp)
        {
            var eventType = TrayUtils.GetEntryEventType(eventId);

            switch (eventType)
            {
                case TrayUtils.EntryEvent.Clicked:
                    entries.FirstOrDefault(p => p.Key == id).Value?.OnActive?.Invoke();
                    break;

                case TrayUtils.EntryEvent.Closed:
                    break;

                case TrayUtils.EntryEvent.Opened:
                    break;

                default:
                    Logger.Log($"未实现的方法被调用：EventAsync: id: {id} | eventId: {eventId} | data: {data} | timestamp: {timestamp}");
                    break;
            }

            return Task.CompletedTask;
        }

        public Task<int[]> EventGroupAsync((int, string, object, uint)[] events)
        {
            Logger.Log($"未实现的方法被调用：EventGroupAsync: {events}");
            throw new NotImplementedException();
        }

        public Task<bool> AboutToShowAsync(int id) => Task.FromResult(true);

        public Task<(int[] updatesNeeded, int[] idErrors)> AboutToShowGroupAsync(int[] ids)
        {
            Logger.Log($"未实现的方法被调用：AboutToShowGroupAsync: {ids}");
            throw new NotImplementedException();
        }

        public event Action<((int, IDictionary<string, object>)[] updatedProps, (int, string[])[] removedProps)> OnEntriesUpdated;

        public Task<IDisposable> WatchItemsPropertiesUpdatedAsync(Action<((int, IDictionary<string, object>)[] updatedProps, (int, string[])[] removedProps)> handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnEntriesUpdated), handler);
        }

        public event Action<(uint revision, int parent)> OnLayoutUpdated;

        public Task<IDisposable> WatchLayoutUpdatedAsync(Action<(uint revision, int parent)> handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnLayoutUpdated), handler);
        }

        public event Action<(int id, uint timestamp)> OnItemActivationRequested;

        public Task<IDisposable> WatchItemActivationRequestedAsync(Action<(int id, uint timestamp)> handler, Action<Exception> onError = null)
        {
            return SignalWatcher.AddAsync(this, nameof(OnItemActivationRequested), handler);
        }

        public Task<object> GetAsync(string prop)
        {
            return Task.FromResult(canonicalProperties.Get(prop));
        }

        public Task<CanonicalDBusMenuProperties> GetAllAsync()
        {
            return Task.FromResult(canonicalProperties);
        }

        public Task SetAsync(string prop, object val)
        {
            throw new NotImplementedException();
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
        }

        public Task<uint> GetVersionAsync()
        {
            return Task.FromResult(canonicalProperties.Version);
        }

        public Task<string> GetTextDirectionAsync()
        {
            return Task.FromResult(canonicalProperties.TextDirection);
        }

        public Task<string> GetStatusAsync()
            => Task.FromResult(canonicalProperties.Status);

        Task<string[]> ICanonicalDBusMenu.GetIconThemePathAsync()
            => Task.FromResult(canonicalProperties.IconThemePath);

        #endregion
    }
}
