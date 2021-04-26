// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Security.Principal;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.Admin
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
            if (isAdmin())
                Notifications.Post(new AdminNotification());
        }

        private bool isAdmin() => OperatingSystem.IsWindows() ? new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) : Mono.Unix.Native.Syscall.geteuid() == 0;

        private class AdminNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public AdminNotification()
            {
                Text = $"Running osu! as {(RuntimeInfo.IsUnix ? "root" : "administrator")} does not improve performance and poses a security risk. Please run the game normally.";
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
