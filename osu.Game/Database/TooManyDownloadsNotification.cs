// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays.Notifications;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Database
{
    public partial class TooManyDownloadsNotification : SimpleNotification
    {
        public TooManyDownloadsNotification()
        {
            Text = BeatmapsetsStrings.DownloadLimitExceeded;
            Icon = FontAwesome.Solid.ExclamationCircle;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IconContent.Colour = colours.RedDark;
        }
    }
}
