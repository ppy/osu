// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Platform.Linux;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MfSettings : SettingsSubsection
    {
        protected override string Header => "Mf-osu";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, OsuConfigManager osuConfig, GameHost host)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用Mf自定义UI",
                    TooltipText = "启用以获得mfosu提供的默认界面体验, "
                                  + "禁用以获得接近原版lazer提供的界面体验",
                    Current = config.GetBindable<bool>(MSetting.OptUI)
                },
                new SettingsCheckbox
                {
                    LabelText = "总是隐藏输入指示器",
                    TooltipText = "如果你的窗口经常无缘无故进入编辑状态,或者只是觉得弹出来烦,那么我建议勾选此项",
                    Current = config.GetBindable<bool>(MSetting.AlwaysHideTextIndicator)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用三角形粒子动画",
                    Current = config.GetBindable<bool>(MSetting.TrianglesEnabled)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用Sayobot功能",
                    TooltipText = "这将影响所有谱面预览、封面、和下图的功能, 但不会影响已完成或正在进行中的请求",
                    Current = config.GetBindable<bool>(MSetting.UseSayobot)
                },
                new SettingsSlider<float>
                {
                    LabelText = "立体音效增益",
                    Current = config.GetBindable<float>(MSetting.SamplePlaybackGain),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "歌曲选择界面背景模糊",
                    Current = config.GetBindable<float>(MSetting.SongSelectBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsCheckbox
                {
                    LabelText = "启动后直接进入选歌界面",
                    TooltipText = "仅在开场样式为\"略过开场\"时生效",
                    Current = config.GetBindable<bool>(MSetting.IntroLoadDirectToSongSelect)
                },
                new SettingsCheckbox
                {
                    LabelText = "使用系统光标",
                    Current = config.GetBindable<bool>(MSetting.UseSystemCursor)
                },
                new SettingsCheckbox
                {
                    LabelText = "使用自定义开屏页背景",
                    Current = config.GetBindable<bool>(MSetting.UseCustomGreetingPicture)
                },
            };

            if (host is LinuxGameHost)
            {
                Add(new SettingsEnumDropdown<GamemodeActivateCondition>
                {
                    LabelText = "Gamemode启用条件",
                    Current = config.GetBindable<GamemodeActivateCondition>(MSetting.Gamemode)
                });
            }
        }
    }
}
