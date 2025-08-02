// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.Security
{
    /// <summary>
    /// Checks if the game is running with elevated privileges (as admin in Windows, root in Unix) and displays a warning notification if so.
    /// </summary>
    public partial class ElevatedPrivilegesChecker : Component
    {
        [Resolved]
        private INotificationOverlay notifications { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Environment.IsPrivilegedProcess)
                notifications.Post(new ElevatedPrivilegesNotification());
        }

        private partial class ElevatedPrivilegesNotification : SimpleNotification
        {
            public ElevatedPrivilegesNotification()
            {
                Text = $"Running osu! as {(RuntimeInfo.IsUnix ? "root" : "administrator")} does not improve performance, may break integrations and poses a security risk. Please run the game as a normal user.";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Icon = FontAwesome.Solid.ShieldAlt;
                IconContent.Colour = colours.YellowDark;
            }
        }
    }
}
