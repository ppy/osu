// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class WebSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "网络";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "在打开外部链接前确认",
                    Current = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                },
                new SettingsCheckbox
                {
                    LabelText = "下图时倾向于不带视频",
                    Keywords = new[] { "no-video" },
                    Current = config.GetBindable<bool>(OsuSetting.PreferNoVideo)
                },
                new SettingsCheckbox
                {
                    LabelText = "旁观时自动下图",
                    Keywords = new[] { "spectator" },
                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating),
                },
                new SettingsCheckbox
                {
                    LabelText = "在搜索结果中显示不合适的内容",
                    Keywords = new[] { "nsfw", "18+", "offensive" },
                    Current = config.GetBindable<bool>(OsuSetting.ShowOnlineExplicitContent),
                }
            };
        }
    }
}
