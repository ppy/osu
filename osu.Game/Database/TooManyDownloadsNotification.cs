// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public class TooManyDownloadsNotification : SimpleNotification
    {
        public TooManyDownloadsNotification(string humanisedModelName)
        {
            Text = CommonStrings.TooManyDownloaded(humanisedModelName);
            Icon = FontAwesome.Solid.ExclamationCircle;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IconBackground.Colour = colours.RedDark;
        }
    }
}
