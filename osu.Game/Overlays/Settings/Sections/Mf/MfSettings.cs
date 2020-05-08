// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Configuration;

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
                    Bindable = config.GetBindable<bool>(MfSetting.OptUI)
                },
                new SettingsCheckbox
                {
                    LabelText = "启用三角形粒子动画",
                    Bindable = config.GetBindable<bool>(MfSetting.TrianglesEnabled)
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
