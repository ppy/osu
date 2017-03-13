// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
            IconBackgound.ColourInfo = ColourInfo.GradientVertical(colours.GreenDark, colours.GreenLight);
        }
    }
}