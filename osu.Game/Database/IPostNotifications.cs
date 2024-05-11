// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public interface IPostNotifications
    {
        /// <summary>
        /// And action which will be fired when a notification should be presented to the user.
        /// </summary>
        public Action<Notification> PostNotification { set; }
    }
}
