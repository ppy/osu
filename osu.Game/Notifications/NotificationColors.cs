// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;

namespace osu.Game.Notifications
{
    public class NotificationColors
    {
        public ColourInfo BackgroundColour { get; set; } = OsuColour.FromHex("FFF");
        public ColourInfo IconBackgroundColour { get; set; } = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.6f));
    }
}
