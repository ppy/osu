// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisUISettings : SettingsSubsection
    {
        protected override string Header => "界面";
        private readonly BindableFloat iR = new BindableFloat();
        private readonly BindableFloat iG = new BindableFloat();
        private readonly BindableFloat iB = new BindableFloat();
        private ColourPreviewer preview;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisInterfaceRed, iR);
            config.BindWith(MSetting.MvisInterfaceGreen, iG);
            config.BindWith(MSetting.MvisInterfaceBlue, iB);

            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = "背景模糊",
                    Current = config.GetBindable<float>(MSetting.MvisBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时的背景亮度",
                    Current = config.GetBindable<float>(MSetting.MvisIdleBgDim),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(红)",
                    Current = iR,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(绿)",
                    Current = iG,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(蓝)",
                    Current = iB,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                preview = new ColourPreviewer(),
                new SettingsCheckbox
                {
                    LabelText = "置顶Proxy",
                    Current = config.GetBindable<bool>(MSetting.MvisStoryboardProxy),
                    TooltipText = "让所有Proxy显示在前景上方"
                },
                new SettingsCheckbox
                {
                    LabelText = "启用背景动画",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableBgTriangles),
                    TooltipText = "如果条件允许,播放器将会在背景显示动画"
                }
            };
        }

        protected override void LoadComplete()
        {
            iR.BindValueChanged(_ => updateColor());
            iG.BindValueChanged(_ => updateColor());
            iB.BindValueChanged(_ => updateColor(), true);
        }

        private void updateColor() => preview.UpdateColor(iR.Value, iG.Value, iB.Value);
    }
}
