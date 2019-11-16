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
        protected override string Header => "主目录";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "界面语音",
                    Bindable = config.GetBindable<bool>(OsuSetting.MenuVoice)
                },
                new SettingsCheckbox
                {
                    LabelText = "osu! 主题音乐",
                    Bindable = config.GetBindable<bool>(OsuSetting.MenuMusic)
                },
                new SettingsDropdown<IntroSequence>
                {
                    LabelText = "开场动画样式",
                    Bindable = config.GetBindable<IntroSequence>(OsuSetting.IntroSequence),
                    Items = Enum.GetValues(typeof(IntroSequence)).Cast<IntroSequence>()
                },
            };
        }
    }
}
