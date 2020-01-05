// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class SongSelectSettings : SettingsSubsection
    {
        protected override string Header => "歌曲选择";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "右键鼠标来快速定位",
                    Bindable = config.GetBindable<bool>(OsuSetting.SongSelectRightMouseScroll),
                },
                new SettingsCheckbox
                {
                    LabelText = "显示转换过的谱面",
                    Bindable = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps),
                },
                new SettingsSlider<double, StarSlider>
                {
                    LabelText = "筛选谱面星级,从",
                    Bindable = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum),
                    KeyboardStep = 0.1f,
                    Keywords = new[] { "star", "difficulty" }
                },
                new SettingsSlider<double, StarSlider>
                {
                    LabelText = "到",
                    Bindable = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum),
                    KeyboardStep = 0.1f,
                    Keywords = new[] { "star", "difficulty" }
                },
                new SettingsEnumDropdown<RandomSelectAlgorithm>
                {
                    LabelText = "随机选择算法",
                    Bindable = config.GetBindable<RandomSelectAlgorithm>(OsuSetting.RandomSelectAlgorithm),
                }
            };
        }

        private class StarSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.## stars");
        }
    }
}
