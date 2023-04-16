using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Logging;

namespace M.DBus.Tray
{
    public static class TrayExtensions
    {
        public static EventType ToEventType(this string message)
        {
            switch (message)
            {
                case "opened":
                    return EventType.Opened;

                case "closed":
                    return EventType.Closed;

                case "clicked":
                    return EventType.Clicked;

                default:
                    return EventType.Unknown;
            }
        }

        /// <summary>
        /// 转换一个SimpleEntry至DBus数据
        /// </summary>
        /// <param name="entry">目标目录</param>
        /// <param name="additionalEntries">所有新发现的子项</param>
        /// <returns>一个能被Tmds.DBus转换的数据</returns>
        public static (int, IDictionary<string, object>, object[]) ToDbusObject(
            this SimpleEntry entry,
            out IDictionary<int, SimpleEntry> additionalEntries)
        {
            lock (entry)
            {
                var result = new Dictionary<string, object>
                {
                    ["type"] = entry.Type,
                    ["label"] = entry.Label,
                    ["enabled"] = entry.Enabled,
                    ["visible"] = entry.Visible,
                    ["icon-name"] = entry.IconName,
                    ["icon-data"] = entry.IconData,
                    ["shortcuts"] = entry.Shortcuts,
                    ["toggle-type"] = entry.ToggleType,
                    ["toggle-state"] = entry.ToggleState,
                    ["children-display"] = entry.ChildrenDisplay
                };

                IList<object> subMenus = new List<object>();

                additionalEntries = new Dictionary<int, SimpleEntry>();

                if (entry.Children.Count <= 0) return (entry.ChildId, result, subMenus.ToArray());

                try
                {
                    SimpleEntry[] entriesCopy = new SimpleEntry[entry.Children.Count];
                    entry.Children.CopyTo(entriesCopy, 0);

                    //遍历所有子菜单
                    foreach (var subEntry in entriesCopy)
                    {
                        //额外产生的SimpleEntry
                        //如果某一个subEntry是SSubmenu，则其Children中的所有SimpleEntry都将加入这个词典
                        IDictionary<int, SimpleEntry> additDict;

                        //添加目录
                        //不需要处理additonalOrders，因为已经有additDict可以用作计数了
                        subMenus.Add(subEntry.ToDbusObject(out additDict));

                        Logger.Log($"添加 -- Adding subEntry {subEntry} for parent {entry}...", level: LogLevel.Debug);

                        foreach (var entryData in additDict)
                            additionalEntries.Add(entryData);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"尝试转换{entry}时出现了错误");
                }

                return (entry.ChildId, result, subMenus.ToArray());
            }
        }

        public static (int, IDictionary<string, object>) ToDbusObject(this SimpleEntry entry)
        {
            int childId = entry.ChildId;

            var result = new Dictionary<string, object>
            {
                ["type"] = entry.Type,
                ["label"] = entry.Label,
                ["enabled"] = entry.Enabled,
                ["visible"] = entry.Visible,
                ["icon-name"] = entry.IconName,
                ["icon-data"] = entry.IconData,
                ["shortcuts"] = entry.Shortcuts,
                ["toggle-type"] = entry.ToggleType,
                ["toggle-state"] = entry.ToggleState,
                ["children-display"] = entry.ChildrenDisplay,
            };

            return (childId, result);
        }
    }
}
