// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MfSettings : SettingsSubsection
    {
        protected override string Header => "Mf-osu";

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config, OsuConfigManager osuConfig)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "导入之前的Mf-osu设置",
                    TooltipText = "导入后的值在下次启动前都将作为对应选项的默认值",
                    Action = () =>
                    {
                        ImportFromOsuConfigManager(config, osuConfig);
                    }
                },
                new SettingsCheckbox
                {
                    LabelText = "启用Mf自定义UI",
                    TooltipText = "启用以获得mfosu提供的默认界面体验, "
                                + "禁用以获得接近原版lazer提供的界面体验",
                    Bindable = config.GetBindable<bool>(MfSetting.OptUI)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用三角形粒子动画",
                    Bindable = config.GetBindable<bool>(MfSetting.TrianglesEnabled)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用Sayobot功能",
                    TooltipText = "这将影响所有谱面预览、封面、和下图的功能, 但不会影响已完成或正在进行中的请求",
                    Bindable = config.GetBindable<bool>(MfSetting.UseSayobot)
                },
                new SettingsSlider<float>
                {
                    LabelText = "立体音效增益",
                    Bindable = config.GetBindable<float>(MfSetting.SamplePlaybackGain),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "歌曲选择界面背景模糊",
                    Bindable = config.GetBindable<float>(MfSetting.SongSelectBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsCheckbox
                {
                    LabelText = "启动后直接进入选歌界面",
                    TooltipText = "仅在开场样式为\"略过动画\"且关闭主题音乐时生效",
                    Bindable = config.GetBindable<bool>(MfSetting.IntroLoadDirectToSongSelect)
                },
            };
        }

        private void ImportFromOsuConfigManager(MfConfigManager mfConfig, OsuConfigManager osuConfig)
        {
            try
            {
                mfConfig.Set(MfSetting.OptUI, osuConfig.Get<bool>(OsuSetting.OptUI));
                mfConfig.Set(MfSetting.TrianglesEnabled, osuConfig.Get<bool>(OsuSetting.TrianglesEnabled));
                mfConfig.Set(MfSetting.MvisParticleAmount, osuConfig.Get<int>(OsuSetting.MvisParticleAmount));
                mfConfig.Set(MfSetting.MvisContentAlpha, osuConfig.Get<float>(OsuSetting.MvisContentAlpha));
                mfConfig.Set(MfSetting.MvisBgBlur, osuConfig.Get<float>(OsuSetting.MvisBgBlur));
                mfConfig.Set(MfSetting.MvisEnableStoryboard, osuConfig.Get<bool>(OsuSetting.MvisEnableStoryboard));
                mfConfig.Set(MfSetting.MvisUseOsuLogoVisualisation, osuConfig.Get<bool>(OsuSetting.MvisUseOsuLogoVisualisation));
                mfConfig.Set(MfSetting.MvisIdleBgDim, osuConfig.Get<float>(OsuSetting.MvisIdleBgDim));
            }
            catch( Exception e )
            {
                Logger.Error(e, "导入设置时出现错误!");
            }
        }
    }
}
