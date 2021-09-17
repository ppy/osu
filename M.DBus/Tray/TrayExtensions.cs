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
        /// <param name="additionalEntries">所有新发现的子项</param>
        /// <returns>一个能被Tmds.DBus转换的数据</returns>
        public static (int, IDictionary<string, object>, object[]) ToDbusObject(
            this SimpleEntry entry,
            int order, int maxOrder,
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
                ["children-display"] = entry.ChildrenDisplay
            };

            IList<object> subMenus = new List<object>();

            additionalEntries = new Dictionary<int, SimpleEntry>();

            if (entry.ChildrenDisplay == ChildrenDisplayType.SSubmenu)
            {
                try
                {
                    //遍历所有子菜单
                    foreach (var subEntry in entry.Children)
                    {
                        //额外产生的SimpleEntry
                        //如果某一个subEntry是SSubmenu，则其Children中的所有SimpleEntry都将加入这个词典
                        IDictionary<int, SimpleEntry> additDict;

                        //如果subEntry没有被指定ChildID
                        if (subEntry.ChildId == -2)
                        {
                            //最大id+1
                            maxOrder++;

                            //设置subEntry的ChildID
                            subEntry.ChildId = maxOrder;

                            //加入要返回的词典中
                            additionalEntries[subEntry.ChildId] = subEntry;

                            //记录
                            //Logger.Log($"{subEntry} 获取了新的ChildId: {subEntry.ChildId}");
                        }

                        //添加目录
                        //不需要处理additonalOrders，因为已经有additDict可以用作计数了
                        subMenus.Add(subEntry.ToDbusObject(subEntry.ChildId, maxOrder, out additDict));

                        //将循环调用返回的 额外词典 加进要返回的 词典 中
                        additionalEntries.AddRange(additDict);

                        //当前的最大id += 多出的目录数量 + 额外目录的数量
                        maxOrder += additDict.Count;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"尝试转换{entry}时出现了错误");
                }
            }

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
