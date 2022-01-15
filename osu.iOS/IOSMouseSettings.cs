// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;

namespace osu.iOS
{
    public class IOSMouseSettings : SettingsSubsection
    {
        protected override LocalisableString Header => MouseSettingsStrings.Mouse;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.DisableMouseWheelVolumeAdjust,
                    TooltipText = MouseSettingsStrings.DisableMouseWheelVolumeAdjustTooltip,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableWheel),
                },
                new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.DisableMouseButtons,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons),
                },
            };
        }
    }
}
