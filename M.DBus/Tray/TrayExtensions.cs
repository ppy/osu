using System;
using System.Collections.Generic;
using System.Linq;
using M.DBus.Utils.Canonical.DBusMenuFlags;
using NuGet.Packaging;
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
        /// <param name="order">这个项目的id</param>
        /// <param name="maxOrder">DBus目录上最后一个项目的id，确保不会发生id冲突</param>
        /// <param name="additionalEntries">添加的子项词典</param>
        /// <param name="additonalOrders">添加了多少个子项</param>
        /// <returns>一个能被Tmds.DBus转换的数据</returns>
        /// <exception cref="InvalidOperationException">某个子项的id被赋予，但是noChildren是false</exception>
        public static (int, IDictionary<string, object>, object[]) ToDbusObject(
            this SimpleEntry entry,
            int order, int maxOrder,
            out int additonalOrders,
            out IDictionary<int, SimpleEntry> additionalEntries)
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
                ["children-display"] = entry.ChildrenDisplay,
            };

            IList<object> subMenus = new List<object>();

            additionalEntries = new Dictionary<int, SimpleEntry>();
            additonalOrders = 0;
            int originalMaxOrder = maxOrder;

            if (entry.ChildrenDisplay == ChildrenDisplayType.SSubmenu)
            {
                try
                {
                    //遍历所有子菜单
                    foreach (var subEntry in entry.Children)
                    {
                        IDictionary<int, SimpleEntry> additDict;
                        int addit;

                        //id自加
                        if (subEntry.ChildId == -2)
                        {
                            maxOrder++;

                            subEntry.ChildId = maxOrder;

                            additionalEntries[subEntry.ChildId] = subEntry;

                            Logger.Log($"{subEntry} 获取了新的ChildId: {subEntry.ChildId}", level: LogLevel.Verbose);
                        }

                        //添加目录
                        subMenus.Add(subEntry.ToDbusObject(subEntry.ChildId, maxOrder, out addit, out additDict));

                        additionalEntries.AddRange(additDict);
                        maxOrder += addit + additDict.Count;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "转换时发生了错误");
                }
            }

            additonalOrders = maxOrder - originalMaxOrder;
            return (order, result, subMenus.ToArray());
        }

        public static (int, IDictionary<string, object>) ToDbusObject(this SimpleEntry entry)
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
                ["children-display"] = entry.ChildrenDisplay,
            };

            return (entry.ChildId, result);
        }
    }
}
