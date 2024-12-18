// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public partial class AlertsAndPrivacySettings : SettingsSubsection
    {
        protected override LocalisableString Header => OnlineSettingsStrings.AlertsAndPrivacyHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.NotifyOnMentioned,
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnUsernameMentioned)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.NotifyOnPrivateMessage,
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnPrivateMessage)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.HideCountryFlags,
                    Current = config.GetBindable<bool>(OsuSetting.HideCountryFlags)
                },
            };
        }
    }
}
