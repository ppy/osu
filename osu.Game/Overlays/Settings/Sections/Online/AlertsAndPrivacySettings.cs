// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class AlertsAndPrivacySettings : SettingsSubsection
    {
        protected override string Header => "提示和隐私";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "当有人在聊天中提到你时显示通知",
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnUsernameMentioned)
                },
                new SettingsCheckbox
                {
                    LabelText = "当有人向你发送私聊信息时显示通知",
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnPrivateMessage)
                },
            };
        }
    }
}
