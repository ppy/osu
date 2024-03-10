// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public partial class MultiplierSettingsSlider : SettingsSlider<double, MultiplierSettingsSlider.MultiplierRoundedSliderBar>
    {
        public MultiplierSettingsSlider()
        {
            KeyboardStep = 0.01f;
        }

        /// <summary>
        /// A slider bar which adds a "x" to the end of the tooltip string.
        /// </summary>
        public partial class MultiplierRoundedSliderBar : RoundedSliderBar<double>
        {
            public override LocalisableString TooltipText => $"{base.TooltipText}x";
        }
    }
}
