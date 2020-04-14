// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class WebSettings : SettingsSubsection
    {
        protected override string Header => "网络";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "再打开外部链接前确认",
                    Bindable = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                },
                new SettingsCheckbox
                {
                    LabelText = "下图时倾向于不带视频",
                    Keywords = new[] { "no-video" },
                    Bindable = config.GetBindable<bool>(OsuSetting.PreferNoVideo)
                },
            };
        }
    }
}
