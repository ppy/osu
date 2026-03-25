// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.MacOS
{
    /// <summary>
    /// Checks if the game is located at `Applications` folder and displays a warning notification if not so.
    /// </summary>
    public partial class MacOSAppLocationChecker : Component
    {
        [Resolved]
        private INotificationOverlay notification { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            string asmPath = RuntimeInfo.EntryAssembly.Location;
            string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string userApp = Path.Combine(userHome, "Applications/");

            bool inRootApp = asmPath.StartsWith("/Applications", StringComparison.Ordinal);
            bool inUserApp = asmPath.StartsWith(userApp, StringComparison.Ordinal);
            if (!(inRootApp || inUserApp))
                notification.Post(new MacOSAppLocationNotification());
        }

        private partial class MacOSAppLocationNotification : SimpleNotification
        {
            public MacOSAppLocationNotification()
            {
                Text = NotificationsStrings.MacOSAppLocation(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
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
