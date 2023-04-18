using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services.Canonical;
using M.DBus.Tray;
using M.DBus.Utils.Canonical.DBusMenuFlags;
using osu.Framework.Development;
using osu.Framework.Logging;
using Tmds.DBus;

#nullable disable

namespace osu.Desktop.DBus.Tray
{
    /// <summary>
    /// todo: 找到文档并实现所有目前未实现的功能<br/>
    /// https://github.com/ubuntu/gnome-shell-extension-appindicator/blob/master/interfaces-xml/DBusMenu.xml<br/>
    /// <br/>
    /// 似乎dde不支持com.canonical.dbusmenu?<br/>
    /// https://github.com/linuxdeepin/dtkwidget/issues/85
    /// </summary>
    public class CanonicalTrayService : IMDBusObject, IDBusMenu, ICloneable
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/MenuBar");

        public string CustomRegisterName => "io.matrix_feather.dbus.menu";
        public bool IsService => true;

        public object Clone()
        {
            var instance = new CanonicalTrayService
            {
                canonicalProperties =
                {
                    Status = this.canonicalProperties.Status,
                    Version = this.canonicalProperties.Version,
                    TextDirection = this.canonicalProperties.TextDirection,
                    IconThemePath = this.canonicalProperties.IconThemePath
                },

                menuRevision = this.menuRevision
            };

            instance.AddEntryRange(this.GetEntries());

            instance.OnEntriesUpdated = this.OnEntriesUpdated;
            instance.OnPropertiesChanged = this.OnPropertiesChanged;
            instance.OnLayoutUpdated = this.OnLayoutUpdated;
            instance.OnItemActivationRequested = this.OnItemActivationRequested;

            return instance;
        }

        #region Canonical DBus

        private readonly DBusMenuProperties canonicalProperties = new DBusMenuProperties();

        private uint menuRevision;

        private readonly RootEntry rootEntry = new RootEntry
        {
            ChildrenDisplay = ChildrenDisplayType.Submenu
        };

        #region 列表物件存储

        private readonly ConcurrentDictionary<int, SimpleEntry> entriesFlatMap = new ConcurrentDictionary<int, SimpleEntry>();
        private readonly ConcurrentDictionary<SimpleEntry, List<SimpleEntry>> entryChildren = new ConcurrentDictionary<SimpleEntry, List<SimpleEntry>>();

        private readonly List<SimpleEntry> rawEntries = new List<SimpleEntry>();

        public SimpleEntry[] GetEntries()
        {
            return rawEntries.ToArray();
        }

        private void addEntry(SimpleEntry entry, bool onlyUpdateEntryIndex = false)
        {
            if (entry is RootEntry)
                throw new InvalidOperationException("不能添加RootEntry");

            Logger.Log($"Adding entry {entry} with id {entry.ChildId} to Index...", level: LogLevel.Debug);

            lock (entriesFlatMap)
            {
                if (!onlyUpdateEntryIndex)
                    rootEntry.Children.Add(entry);

                entriesFlatMap[entry.ChildId] = entry;

                //当此Child属性发生改变时更新Layout
                entry.OnPropertyChanged = () =>
                {
                    Logger.Log($"OnChanged: {entry}, ChildModified: {entry.ChildModified}", level: LogLevel.Debug);

                    //如果子目录发生改变，则更新子目录
                    if (entry.ChildModified)
                    {
                        var children = getChildren(entry);
                        var oldChildren = entryChildren.GetValueOrDefault(entry);

                        if (oldChildren != null)
                        {
                            //先从flatMap中移除所有旧的Entry
                            foreach (var oldChild in oldChildren)
                                entriesFlatMap.TryRemove(oldChild.ChildId, out _);
                        }

                        //然后再添加到Flatmap中
                        foreach (var childEntry in children)
                        {
                            childEntry.ParentId = entry.ChildId;
                            addEntry(childEntry, true);
                        }
                    }

                    notifyLayoutUpdate(entry);
                };

                if (entry.Children.Count <= 0) return;

                var children = this.getChildren(entry);
                this.entryChildren[entry] = children.ToList();

                foreach (var simpleEntry in this.getChildren(entry))
                    this.addEntry(simpleEntry, true);
            }
        }

        public void AddEntryRange(SimpleEntry[] entries)
        {
            foreach (var entry in entries)
            {
                rawEntries.Add(entry);
                addEntry(entry);
            }

            notifyLayoutUpdate();
        }

        public void AddEntryToMenu(SimpleEntry entry)
        {
            rawEntries.Add(entry);
            addEntry(entry);
            notifyLayoutUpdate();
        }

        public void RemoveEntryFromMenu(SimpleEntry entry)
        {
            lock (entriesFlatMap)
            {
                rawEntries.Remove(entry);

                var key = entriesFlatMap.FirstOrDefault(p => p.Value == entry);

                if (key.Value != null)
                {
                    entriesFlatMap.Remove(key.Key, out _);
                    rootEntry.Children.Remove(key.Value);

                    var children = entryChildren.GetValueOrDefault(entry);

                    if (children != null)
                    {
                        foreach (var simpleEntry in children)
                            entriesFlatMap.TryRemove(simpleEntry.ChildId, out _);
                    }
                }
                else
                    throw new InvalidOperationException($"给定的 {entry} 不在列表中");

                notifyLayoutUpdate();
            }
        }

        private void notifyLayoutUpdate(SimpleEntry entry = null)
        {
            invalidateLayout();
            menuRevision++;

            lock (entriesFlatMap)
            {
                entry ??= rootEntry;
            }

            int childId = entry.ChildId;

            if (entry.ChildrenDisplay == ChildrenDisplayType.Submenu)
            {
                OnLayoutUpdated?.Invoke((menuRevision, childId));
            }
            else
            {
                OnEntriesUpdated?.Invoke((new[]
                {
                    entry.ToDbusObject()
                }, Array.Empty<(int, string[])>()));
            }
        }

        #endregion

        private (uint revision, (int, IDictionary<string, object>, object[]) layout) cachedLayout;

        private void invalidateLayout()
        {
            this.layoutValid = false;
        }

        private bool layoutValid;

        public Task<(uint revision, (int, IDictionary<string, object>, object[]) layout)> GetLayoutAsync(int parentId, int recursionDepth, string[] propertyNames)
        {
            Logger.Log($"方法被调用：GetLayoutAsync: parentId: {parentId} | recursionDepth: {recursionDepth}", level: LogLevel.Debug);

            try
            {
                SimpleEntry target;

                lock (entriesFlatMap)
                {
                    target = parentId == 0
                        ? rootEntry
                        : entriesFlatMap.FirstOrDefault(e => e.Key == parentId).Value;
                }

                var result = layoutValid ? cachedLayout : (menuRevision, target.ToDbusObject(out _));
                this.cachedLayout = result;
                layoutValid = true;

                Logger.Log($"Result: REVISION: {menuRevision} :: EntryID: {result.Item2.Item1} :: SubMenusSize: {result.Item2.Item3.Length}", level: LogLevel.Debug);

                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                Logger.Error(e, "执行GetLayout时出现错误");
                return null;
            }
        }

        private List<SimpleEntry> getChildren(SimpleEntry entry)
        {
            var list = new List<SimpleEntry>();

            if (entry.Children.Count == 0) return list;

            foreach (var simpleEntry in entry.Children)
            {
                list.Add(simpleEntry);

                if (simpleEntry.Children.Count > 0)
                    list.AddRange(getChildren(simpleEntry));
            }

            return list;
        }

        public Task<(int, IDictionary<string, object>)[]> GetGroupPropertiesAsync(int[] ids, string[] propertyNames)
        {
            Logger.Log("方法被调用：GetGroupPropertiesAsync: ids:", level: LogLevel.Debug);
            //foreach (var id in ids) Logger.Log(id.ToString());

            try
            {
                var result = new List<(int, IDictionary<string, object>)>();

                foreach (int id in ids)
                {
                    var target = entriesFlatMap.FirstOrDefault(k => k.Key == id);
                    if (target.Value == null) continue;

                    result.Add(target.Value.ToDbusObject());
                }

                return Task.FromResult(result.ToArray());
            }
            catch (Exception e)
            {
                Logger.Error(e, "执行GetGroupPropertiesAsync时出现错误");
                return null;
            }
        }

        public Task<object> GetPropertyAsync(int id, string name)
        {
            Logger.Log($"未实现的方法被调用：GetPropertyAsync: id: {id} | name: {name}");
            return Task.FromException<object>(new NotImplementedException("未实现的接口"));
        }

        public Task EventAsync(int id, string eventId, object data, uint timestamp)
        {
            var eventType = eventId.ToEventType();

            Logger.Log($"Event Async -> {id} :: {eventId} :: {data} :: {timestamp}", level: LogLevel.Debug);

            switch (eventType)
            {
                case EventType.Clicked:
                    SimpleEntry target = null;

                    try
                    {
                        target = entriesFlatMap.FirstOrDefault(p => p.Key == id).Value;
                        target?.OnActive?.Invoke();

                        if (DebugUtils.IsDebugBuild)
                        {
                            Logger.Log($"Get target entry: {target}");

                            foreach (var keyValuePair in entriesFlatMap)
                                Logger.Log($"Entries :: Id: {keyValuePair.Key} --> {keyValuePair.Value.ChildId} :: {keyValuePair.Value.Label}");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"激活 {target?.ToString() ?? "未知菜单"} 的动作时出现问题, 请尝试联系其内容提供方");
                    }

                    break;

                case EventType.Closed:
                    break;

                case EventType.Opened:
                    break;

                default:
                    Logger.Log($"未实现的方法被调用：EventAsync: id: {id} | eventId: {eventId} | data: {data} | timestamp: {timestamp}");
                    break;
            }

            return Task.CompletedTask;
        }

        public Task<int[]> EventGroupAsync((int, string, object, uint)[] events)
        {
            foreach (var obj in events)
                EventAsync(obj.Item1, obj.Item2, obj.Item3, obj.Item4);

            Logger.Log($"未完全实现的方法被调用：EventGroupAsync: {events}");
            return Task.FromResult(Array.Empty<int>());
        }

        public Task<bool> AboutToShowAsync(int id)
        {
            try
            {
                //bool returnValue = id == 0 || entriesFlatMap.ContainsKey(id);
                //Logger.Log($"方法被调用: AboutToShowAsync id: {id} -> {returnValue}", level: LogLevel.Debug);

                return Task.FromResult(false);
            }
            catch (Exception e)
            {
                Logger.Error(e, "???");
            }

            return Task.FromResult(false);
        }

        public Task<(int[] updatesNeeded, int[] idErrors)> AboutToShowGroupAsync(int[] ids)
        {
            Logger.Log($"未实现的方法被调用：AboutToShowGroupAsync: {ids}");
            return Task.FromResult((Array.Empty<int>(), Array.Empty<int>()));
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

        public Task<DBusMenuProperties> GetAllAsync()
        {
            return Task.FromResult(canonicalProperties);
        }

        public Task SetAsync(string prop, object val)
        {
            SetProperty(prop, val);
            return Task.CompletedTask;
        }

        internal void SetProperty(string prop, object value)
        {
            if (canonicalProperties.Set(prop, value))
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(prop, value));
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
        {
            return Task.FromResult(canonicalProperties.Status);
        }

        Task<string[]> IDBusMenu.GetIconThemePathAsync()
        {
            return Task.FromResult(canonicalProperties.IconThemePath);
        }

        #endregion
    }
}
