// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An overlay which is capable of showing notifications to the user.
    /// </summary>
    [Cached]
    public interface INotificationOverlay
    {
        /// <summary>
        /// Post a new notification for display.
        /// </summary>
        /// <param name="notification">The notification to display.</param>
        void Post(Notification notification);

        /// <summary>
        /// Hide the overlay, if it is currently visible.
        /// </summary>
        void Hide();

        /// <summary>
        /// Current number of unread notifications.
        /// </summary>
        IBindable<int> UnreadCount { get; }

        /// <summary>
        /// Whether there are any ongoing operations, such as imports or downloads.
        /// </summary>
        public bool HasOngoingOperations => OngoingOperations.Any();

        /// <summary>
        /// All current displayed notifications, whether in the toast tray or a section.
        /// </summary>
        IEnumerable<Notification> AllNotifications { get; }

        /// <summary>
        /// All ongoing operations (ie. any <see cref="ProgressNotification"/> not in a completed or cancelled state).
        /// </summary>
        public IEnumerable<ProgressNotification> OngoingOperations => AllNotifications.OfType<ProgressNotification>().Where(p => p.Ongoing);
    }
}
