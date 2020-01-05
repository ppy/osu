// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class MainMenuSettings : SettingsSubsection
    {
        protected override string Header => "主界面";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "开场语音",
                    Bindable = config.GetBindable<bool>(OsuSetting.MenuVoice)
                },
                new SettingsCheckbox
                {
                    LabelText = "osu! 主题音乐",
                    Bindable = config.GetBindable<bool>(OsuSetting.MenuMusic)
                },
                new SettingsDropdown<IntroSequence>
                {
                    LabelText = "开场样式",
                    Bindable = config.GetBindable<IntroSequence>(OsuSetting.IntroSequence),
                    Items = Enum.GetValues(typeof(IntroSequence)).Cast<IntroSequence>()
                },
                new SettingsDropdown<BackgroundSource>
                {
                    LabelText = "背景来源",
                    TooltipText = "需要成为osu!supporter来开启",
                    Bindable = config.GetBindable<BackgroundSource>(OsuSetting.MenuBackgroundSource),
                    Items = Enum.GetValues(typeof(BackgroundSource)).Cast<BackgroundSource>()
                }
            };
        }
    }
}
