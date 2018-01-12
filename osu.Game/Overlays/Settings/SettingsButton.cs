// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsButton : TriangleButton
    {
        public SettingsButton()
        {
            RelativeSizeAxes = Axes.X;
            Padding = new MarginPadding { Left = SettingsOverlay.CONTENT_MARGINS, Right = SettingsOverlay.CONTENT_MARGINS };
        }
    }
}
