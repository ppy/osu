// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Overlays.Notifications
{
    public class ProgressCompletionNotification : SimpleNotification
    {
        public ProgressCompletionNotification()
        {
            Icon = FontAwesome.fa_check;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IconBackgound.Colour = ColourInfo.GradientVertical(colours.GreenDark, colours.GreenLight);
        }
    }
}
