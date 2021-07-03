// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class OffsetSettings : SettingsSubsection
    {
        protected override string Header => "偏移量调整";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double, OffsetSlider>
                {
                    LabelText = "音频偏移量",
                    Current = config.GetBindable<double>(OsuSetting.AudioOffset),
                    KeyboardStep = 1f
                },
                new SettingsButton
                {
                    Text = "设置向导"
                }
            };
        }

        private class OffsetSlider : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Value.ToString(@"0ms");
        }
    }
}
