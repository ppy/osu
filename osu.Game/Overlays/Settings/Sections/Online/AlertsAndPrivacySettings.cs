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
                    LabelText = "Show a notification popup when someone says your name",
                    Current = config.GetBindable<bool>(OsuSetting.ChatHighlightName)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show private message notifications",
                    Current = config.GetBindable<bool>(OsuSetting.ChatMessageNotification)
                },
            };
        }
    }
}
