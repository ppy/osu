// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A slider bar which displays a millisecond time value.
    /// </summary>
    public class TimeSlider : OsuSliderBar<double>
    {
        protected override LocalisableString GetTooltipText(double value) => $"{value:N0} ms";
    }
}
