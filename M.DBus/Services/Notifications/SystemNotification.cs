using System;
using System.Collections.Generic;
using M.DBus.Utils;

namespace M.DBus.Services.Notifications
{
    public class SystemNotification
    {
        /// <summary>
        /// 应用名称，可以是空
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 图标名称<br/>
        /// https://specifications.freedesktop.org/notification-spec/latest/ar01s05.html
        /// </summary>
        public string IconName { get; set; } = string.Empty;

        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// https://specifications.freedesktop.org/notification-spec/latest/ar01s08.html
        /// </summary>
        public IDictionary<string, object> Hints { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 存在时间（毫秒）
        /// </summary>
        public int DisplayTime = 2000;

        /// <summary>
        /// 通知附带的选项
        /// </summary>
        public NotificationAction[] Actions { get; set; } = Array.Empty<NotificationAction>();

        /// <summary>
        /// 通知被关闭时要执行的动作
        /// </summary>
        public Action<CloseReason> OnClosed;

        public override string ToString() => $"'{Name}': '{Title}' - '{Description}'";
    }
}
