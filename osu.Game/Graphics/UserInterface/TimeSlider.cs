// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A slider bar which displays a millisecond time value.
    /// </summary>
    public class TimeSlider : OsuSliderBar<double>
    {
        public override LocalisableString TooltipText => $"{Current.Value:N0} ms";
    }
}
