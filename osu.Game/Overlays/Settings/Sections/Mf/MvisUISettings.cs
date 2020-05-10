// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MvisUISettings : SettingsSubsection
    {
        protected override string Header => "界面";

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = "背景模糊",
                    Bindable = config.GetBindable<float>(MfSetting.MvisBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时的背景暗化",
                    Bindable = config.GetBindable<float>(MfSetting.MvisIdleBgDim),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时的M-vis面板不透明度",
                    Bindable = config.GetBindable<float>(MfSetting.MvisContentAlpha),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsCheckbox
                {
                    LabelText = "启用故事版/背景视频",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisEnableStoryboard),
                }
            };
        }
    }
}
