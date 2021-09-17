using System;
using System.Diagnostics.CodeAnalysis;

namespace M.DBus.Services.Notifications
{
    public class NotificationAction
    {
        /// <summary>
        /// 此选项的标识<br/>
        /// 详见https://specifications.freedesktop.org/notification-spec/latest/ar01s09.html中的"Table 6. Notify Parameters"
        /// </summary>
        [NotNull]
        public string Id { get; }

        //todo: 适配LocalisableString?
        /// <summary>
        /// 此选项的本地化字符串<br/>
        /// 详见https://specifications.freedesktop.org/notification-spec/latest/ar01s09.html中的"Table 6. Notify Parameters"
        /// </summary>
        [NotNull]
        public string Description;

        /// <summary>
        /// 当选项被激活时要执行的动作
        /// </summary>
        public Action OnInvoked;

        public NotificationAction(string id, string description = null, Action onInvoked = null)
        {
            if (string.IsNullOrEmpty(description)) description = id;

            Id = id;
            Description = description;
            OnInvoked = onInvoked;
        }
    }
}
