// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class VolumeScrollMultiplierSettings : SettingsSubsection
    {
        protected override LocalisableString Header => AudioSettingsStrings.VolumeScrollMultiplier;

        public override IEnumerable<string> FilterTerms => base.FilterTerms.Concat(new[] { "volume", "multiplier", "scroll" });

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double, OffsetSlider>
                {
                    LabelText = AudioSettingsStrings.VolumeScrollMultiplier,
                    Current = config.GetBindable<double>(OsuSetting.VolumeScrollMultiplier),
                    KeyboardStep = 1f
                }
            };
        }

        private class OffsetSlider : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Value.ToString(@"0.#x");
        }
    }
}
