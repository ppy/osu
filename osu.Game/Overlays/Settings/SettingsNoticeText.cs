// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Settings
{
    public class SettingsNoticeText : LinkFlowContainer
    {
        public SettingsNoticeText(OsuColour colours)
            : base(s => s.Colour = colours.Yellow)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }
    }
}
