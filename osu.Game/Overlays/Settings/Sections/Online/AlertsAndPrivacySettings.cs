// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class AlertsAndPrivacySettings : SettingsSubsection
    {
        protected override string Header => "Alerts and Privacy";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show a notification when someone mentions your name",
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnUsernameMentioned)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show a notification when you receive a private message",
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnPrivateMessage)
                },
            };
        }
    }
}
