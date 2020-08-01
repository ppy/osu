// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MvisVisualSettings : SettingsSubsection
    {
        protected override string Header => "特效";

        private SettingsCheckbox customColourCheckbox;
        private Container resizableContainer;

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "显示粒子效果",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisShowParticles)
                },
                new SettingsSlider<int>
                {
                    LabelText = "屏幕粒子数",
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(MfSetting.MvisParticleAmount),
                    KeyboardStep = 1,
                },
                new SettingsEnumDropdown<MvisBarType>
                {
                    LabelText = "频谱类型",
                    Bindable = config.GetBindable<MvisBarType>(MfSetting.MvisBarType)
                },
                new SettingsSlider<int>
                {
                    LabelText = "分段数",
                    Bindable = config.GetBindable<int>(MfSetting.MvisVisualizerAmount),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "宽度",
                    Bindable = config.GetBindable<double>(MfSetting.MvisBarWidth),
                    KeyboardStep = 0.1f
                },
                new SettingsSlider<int>
                {
                    LabelText = "每个分段的频谱密度",
                    Bindable = config.GetBindable<int>(MfSetting.MvisBarsPerVisual),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<int>
                {
                    LabelText = "频谱旋转角度",
                    KeyboardStep = 1,
                    Bindable = config.GetBindable<int>(MfSetting.MvisRotation)
                },
                customColourCheckbox = new SettingsCheckbox
                {
                    LabelText = "使用自定义颜色",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisUseCustomColour)
                },
                resizableContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            new SettingsSlider<int>
                            {
                                LabelText = "红",
                                KeyboardStep = 1,
                                Bindable = config.GetBindable<int>(MfSetting.MvisRed)
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "绿",
                                KeyboardStep = 1,
                                Bindable = config.GetBindable<int>(MfSetting.MvisGreen)
                            },
                            new SettingsSlider<int>
                            {
                                KeyboardStep = 1,
                                LabelText = "蓝",
                                Bindable = config.GetBindable<int>(MfSetting.MvisBlue)
                            }
                        }
                    }
                },
                new SettingsCheckbox
                {
                    LabelText = "使用原版频谱效果",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisUseOsuLogoVisualisation),
                },
            };
        }

        protected override void LoadComplete()
        {
            customColourCheckbox.Bindable.BindValueChanged(useCustom =>
            {
                if (useCustom.NewValue)
                {
                    resizableContainer.ClearTransforms();
                    resizableContainer.AutoSizeAxes = Axes.Y;
                }
                else
                {
                    resizableContainer.AutoSizeAxes = Axes.None;
                    resizableContainer.ResizeHeightTo(0, 200, Easing.OutQuint);
                }
            }, true);

            resizableContainer.FinishTransforms();
        }
    }
}
