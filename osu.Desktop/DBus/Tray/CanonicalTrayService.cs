using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using M.DBus;
using M.DBus.Services.Canonical;
using M.DBus.Tray;
using M.DBus.Utils.Canonical.DBusMenuFlags;
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
    public class CanonicalTrayService : IMDBusObject, IDBusMenu
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/MenuBar");

        public string CustomRegisterName => "io.matrix_feather.dbus.menu";

        #region Canonical DBus

        private readonly DBusMenuProperties canonicalProperties = new DBusMenuProperties();

        private uint menuRevision;

        private readonly RootEntry rootEntry = new RootEntry
        {
            ChildrenDisplay = ChildrenDisplayType.Submenu
        };

        #region 列表物件存储

        private readonly Dictionary<int, SimpleEntry> entries = new Dictionary<int, SimpleEntry>();

        private int lastEntryId = 1;

        private void addEntry(SimpleEntry entry)
        {
            if (entry is RootEntry)
                throw new InvalidOperationException("不能添加RootEntry");

            lock (entries)
            {
                rootEntry.Children.Add(entry);
                entries[lastEntryId] = entry;
                entry.OnPropertyChanged = () => triggerLayoutUpdate(entry);

                lastEntryId++;
            }
        }

        public void AddEntryRange(SimpleEntry[] entries)
        {
            foreach (var entry in entries) addEntry(entry);

            triggerLayoutUpdate();
        }

        public void AddEntryToMenu(SimpleEntry entry)
        {
            addEntry(entry);
            triggerLayoutUpdate();
        }

        public void RemoveEntryFromMenu(SimpleEntry entry)
        {
            lock (entries)
            {
                var key = entries.FirstOrDefault(p => p.Value == entry);

                if (key.Value != null)
                {
                    entries.Remove(key.Key);
                    rootEntry.Children.Remove(key.Value);
                }
                else
                    throw new InvalidOperationException($"给定的 {entry} 不在列表中");

                triggerLayoutUpdate();
            }
        }

        private void triggerLayoutUpdate(SimpleEntry entry = null)
        {
            menuRevision++;

            entry ??= rootEntry;
            //Logger.Log($"{entry.ToString().Replace("\n", "\\n")}发生了变化");

            if (entry.ChildId == null) throw new ArgumentNullException($"{entry}没有ChildID");

            int childId = (int)entry.ChildId;

            updatedEntries[childId] = entry;

            if (entry.ChildrenDisplay == ChildrenDisplayType.Submenu)
                OnLayoutUpdated?.Invoke((menuRevision, childId));
            else
            {
                OnEntriesUpdated?.Invoke((new[]
                {
                    entry.ToDbusObject()
                }, Array.Empty<(int, string[])>()));
            }
        }

        #endregion

        private int dbusItemMaxOrder;
        private readonly Dictionary<int, SimpleEntry> updatedEntries = new Dictionary<int, SimpleEntry>();
        private (uint menuRevision, (int, IDictionary<string, object>, object[])) cachedLayoutObject;

        public Task<(uint revision, (int, IDictionary<string, object>, object[]) layout)> GetLayoutAsync(int parentId, int recursionDepth, string[] propertyNames)
        {
            //Logger.Log($"方法被调用：GetLayoutAsync: parentId: {parentId} | recursionDepth: {recursionDepth}", level: LogLevel.Verbose);

            try
            {
                (uint menuRevision, (int, IDictionary<string, object>, object[])) result;

                if (updatedEntries.Count != 0)
                {
                    //Logger.Log("刷新DBus目录缓存", level: LogLevel.Verbose);
                    IDictionary<int, SimpleEntry> additDict;
                    //int addit;

                    Debug.Assert(rootEntry.ChildId != null, "rootEntry.ChildId != null");

                    result = (menuRevision, rootEntry.ToDbusObject(
                        (int)rootEntry.ChildId,
                        dbusItemMaxOrder,
                        out additDict));

                    //缓存当前结果到cachedLayoutObject中
                    cachedLayoutObject = result;

                    //更新最大id
                    dbusItemMaxOrder += additDict.Count;

                    //递归检查所有新增的SimpleEntry
                    lock (entries)
                    {
                        foreach (var kvp in additDict)
                        {
                            //尝试添加到entries中, 如果成功, 订阅OnPropertyChanged
                            if (entries.TryAdd(kvp.Key, kvp.Value))
                                kvp.Value.OnPropertyChanged = () => triggerLayoutUpdate(kvp.Value);
                        }
                    }

                    //因为Layout已经更新，updatedEntries中现有的值已经不需要了，故对其清空
                    updatedEntries.Clear();
                }

                //如果parentID是0, 返回缓存的列表
                if (parentId == 0)
                    return Task.FromResult(cachedLayoutObject);

                var target = entries.FirstOrDefault(e => e.Key == parentId).Value;

                if (target.ChildId == null) throw new ArgumentNullException($"{target}的ChildID是null");

                result = (menuRevision, target.ToDbusObject(
                    (int)target.ChildId,
                    int.MinValue, //转换在上方已经完成了，因此在这里不要传递dbusItemMaxOrder
                    out _)); //同上

                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                Logger.Error(e, "执行GetLayout时出现错误");
                return null;
            }
        }

        public Task<(int, IDictionary<string, object>)[]> GetGroupPropertiesAsync(int[] ids, string[] propertyNames)
        {
            Logger.Log("方法被调用：GetGroupPropertiesAsync: ids:", level: LogLevel.Verbose);
            //foreach (var id in ids) Logger.Log(id.ToString());

            try
            {
                var result = new List<(int, IDictionary<string, object>)>();

                foreach (int id in ids)
                {
                    var target = entries.FirstOrDefault(k => k.Key == id);
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

            switch (eventType)
            {
                case EventType.Clicked:
                    SimpleEntry target = null;

                    try
                    {
                        target = entries.FirstOrDefault(p => p.Key == id).Value;
                        target.OnActive?.Invoke();
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
            {
                EventAsync(obj.Item1, obj.Item2, obj.Item3, obj.Item4);
            }

            Logger.Log($"未完全实现的方法被调用：EventGroupAsync: {events}");
            return Task.FromResult(Array.Empty<int>());
        }

        public Task<bool> AboutToShowAsync(int id)
        {
            Logger.Log($"方法被调用: AboutToShowAsync id: {id}", level: LogLevel.Verbose);

            var returnValue = updatedEntries.ContainsKey(id);

            return Task.FromResult(returnValue);
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
