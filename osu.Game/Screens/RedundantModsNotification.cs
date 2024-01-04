// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Localisation;

namespace osu.Game.Screens
{
    public partial class RedundantModsNotification : SimpleNotification
    {
        public override bool IsImportant => true;

        public RedundantModsNotification(string[] removedMods)
        {
            Text = NotificationsStrings.RedundantModsRemoved(string.Join(", ", removedMods));
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, INotificationOverlay notificationOverlay)
        {
            IconContent.Colour = colours.YellowDark;
        }
    }
}
