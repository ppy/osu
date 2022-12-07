// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    }
}
