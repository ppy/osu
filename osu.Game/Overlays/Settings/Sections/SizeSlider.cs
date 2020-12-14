// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections
{
    /// <summary>
    /// A slider intended to show a "size" multiplier number, where 1x is 1.0.
    /// </summary>
    internal class SizeSlider : OsuSliderBar<float>
    {
        public override string TooltipText => Current.Value.ToString(@"0.##x");
    }
}
