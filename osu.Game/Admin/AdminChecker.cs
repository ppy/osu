// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Admin
{
    /// <summary>
    /// Checks if the game is running with elevated privileges (as admin in Windows, root in Unix) and displays a warning notification if so.
    /// </summary>
    public class AdminChecker : CompositeDrawable
    {
        [Resolved]
        protected NotificationOverlay Notifications { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (IsAdmin())
                Notifications.Post(new AdminNotification());
        }

        protected virtual bool IsAdmin() => false;

        private class AdminNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public AdminNotification()
            {
                bool isUnix = RuntimeInfo.IsUnix;
                Text = $"Running osu! as {(isUnix ? "root" : "administrator")} does not improve performance and poses a security risk. Please run the game normally.";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, NotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.ShieldAlt;
                IconBackgound.Colour = colours.YellowDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    return true;
                };
            }
        }
    }


}
