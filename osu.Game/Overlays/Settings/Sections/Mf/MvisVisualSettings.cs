// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MvisVisualSettings : SettingsSubsection
    {
        protected override string Header => "Mvis播放器 - 特效";

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<int>
                {
                    LabelText = "屏幕粒子数",
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(MfSetting.MvisParticleAmount),
                    KeyboardStep = 1,
                },
                new SettingsCheckbox
                {
                    LabelText = "使用原版频谱效果",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisUseOsuLogoVisualisation),
                },
                new SettingsCheckbox
                {
                    LabelText = "RGB光效",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisBarRGBLighting),
                },
                new SettingsCheckbox
                {
                    LabelText = "打砖块",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisEnableBrick),
                },
                new SettingsSlider<int>
                {
                    LabelText = "频谱密度",
                    Bindable = config.GetBindable<int>(MfSetting.MvisBarCount),
                    TransferValueOnCommit = true,
                    KeyboardStep = 1,
                },
            };
        }
    }
}
