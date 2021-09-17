using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace M.DBus.Services.Canonical
{
    [DBusInterface("com.canonical.dbusmenu")]
    public interface IDBusMenu : IDBusObject
    {
        #region 函数

        /// <summary>
        /// 提供附加到布局中条目的布局和属性。
        /// 它只给出在 parentId 中指定的项目的子项目。
        /// 它将根据 propertyNames 中的值返回所有属性或特定属性。
        /// 格式是递归的，其中第二个“v”与原始“a(ia{sv}av)”的格式相同。
        /// 它的内容取决于 recursionDepth 的值。
        /// </summary>
        ///
        /// <param name="parentId">
        /// 布局的父节点的 ID。 要从根节点获取布局，请使用0。
        /// </param>
        ///
        /// <param name="recursionDepth">
        /// 要使用的递归级别的数量。 这会影响第二个变体数组的内容。
        /// - -1：发送 parentId 下的所有项目。
        /// - 0：不递归，数组将为空。
        /// - n：数组将包含高达“n”级深度的项目。
        /// </param>
        ///
        /// <param name="propertyNames">
        /// 我们感兴趣的项目属性列表。
        /// 如果列表中没有条目，则将发送所有属性。
        /// </param>
        ///
        /// <returns>
        /// revision: 布局的修订号。 用于匹配 layoutUpdated 信号。<br/>
        /// layout: 布局，作为递归结构。</returns>
        Task<(uint revision, (int, IDictionary<string, object>, object[]) layout)> GetLayoutAsync(int parentId, int recursionDepth, string[] propertyNames);

        /// <summary>
        /// 返回作为 parentId 子项的项目列表。
        /// </summary>
        ///
        /// <param name="ids">
        /// 我们应该在其上查找属性的 id 列表。
        /// 如果列表为空，则应发送所有菜单项。
        /// </param>
        ///
        /// <param name="propertyNames">
        /// 我们感兴趣的项目属性列表。
        /// 如果列表中没有条目，则将发送所有属性。
        /// </param>
        ///
        /// <returns>
        /// 一组属性值。<br/>
        /// 此区域中的项目表示为遵循以下格式的结构：<br/>
        /// ids: 未签名项目ID<br/>
        /// propertyNames: 映射（字符串 => 变体）请求的项目属性
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        Task<(int, IDictionary<string, object>)[]> GetGroupPropertiesAsync(int[] ids, string[] propertyNames);

        /// <summary>
        /// 获取单个项目的信号属性。
        /// 如果您要实现此接口，则这没有用，只有在通过命令行工具进行调试时才应使用它。
        /// </summary>
        /// <param name="id">接收事件的项目的 id</param>
        /// <param name="name">要获取的属性的名称</param>
        /// <returns>目标属性的值</returns>
        /// <exception cref="NotImplementedException"></exception>
        Task<object> GetPropertyAsync(int id, string name);

        /// <summary>
        /// <![CDATA[小程序调用它来通知应用程序在菜单项上发生了事件。
        /// 类型可以是以下之一：
        /// “clicked”
        /// “hovered”
        ///
        /// 供应商特定事件可以添加前缀“x-<vendor>-”]]>
        /// </summary>
        /// <param name="id">接收事件的项目的 id</param>
        /// <param name="eventId">事件类型</param>
        /// <param name="data">事件特定数据</param>
        /// <param name="timestamp">事件发生的时间（如果可用）或消息发送的时间（如果不可用）</param>
        /// <returns></returns>
        Task EventAsync(int id, string eventId, object data, uint timestamp);

        /// <summary>
        /// 无文档描述。
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task<int[]> EventGroupAsync((int, string, object, uint)[] events);

        /// <summary>
        /// 由其他程序调用它来通知应用程序即将在指定项目下显示菜单。
        /// </summary>
        /// <param name="id">
        /// 哪个菜单项代表将要显示的项的父项。<br/>
        /// (Which menu item represents the parent of the item about to be shown.)
        /// </param>
        /// <returns>这个 AboutToShow 事件是否应该导致菜单被更新。</returns>
        Task<bool> AboutToShowAsync(int id);

        /// <summary>
        /// 无文档描述<br/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<(int[] updatesNeeded, int[] idErrors)> AboutToShowGroupAsync(int[] ids);

        #endregion

        #region 信号

        /// <summary>
        /// 当许多项目有大量属性更新时触发，因此它们都被分组到单个 dbus 消息中。
        /// 格式是项目的 ID，带有这些属性的名称和值的哈希表。
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="onError"></param>
        /// <returns>
        /// updatedProps: 要更新的属性<br/>
        /// removedProps: 被删除的属性
        /// </returns>
        Task<IDisposable> WatchItemsPropertiesUpdatedAsync(Action<((int, IDictionary<string, object>)[] updatedProps, (int, string[])[] removedProps)> handler, Action<Exception> onError = null);

        /// <summary>
        /// 由应用程序触发以通知布局更新的显示，直至修订
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="onError"></param>
        /// <returns>
        /// revision: 我们目前正在进行的布局的修订<br/>
        /// parent: 如果布局更新仅针对子树，则这是已更改条目的父项。 如果整个布局应被视为无效，则为零。
        /// </returns>
        Task<IDisposable> WatchLayoutUpdatedAsync(Action<(uint revision, int parent)> handler, Action<Exception> onError = null);

        /// <summary>
        /// 服务器请求所有显示此菜单的客户端向用户打开它。
        /// 这将用于诸如热键之类的东西，当用户按下它们时，菜单应该打开并向用户显示。
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="onError"></param>
        /// <returns>
        /// id: 应激活的菜单 ID<br/>
        /// timestamp: 事件发生的时间
        /// </returns>
        Task<IDisposable> WatchItemActivationRequestedAsync(Action<(int id, uint timestamp)> handler, Action<Exception> onError = null);

        Task<object> GetAsync(string prop);
        Task<DBusMenuProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);

        #endregion

        //Extensions
        Task<uint> GetVersionAsync();
        Task<string> GetTextDirectionAsync();
        Task<string> GetStatusAsync();
        Task<string[]> GetIconThemePathAsync();
    }
}
