// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisVisualSettings : SettingsSubsection
    {
        protected override string Header => "视觉效果";

        private SettingsCheckbox customColourCheckbox;
        private Container resizableContainer;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "无故事版可用时显示背景动画",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableBgTriangles),
                },
                new SettingsCheckbox
                {
                    LabelText = "显示粒子",
                    Current = config.GetBindable<bool>(MSetting.MvisShowParticles)
                },
                new SettingsSlider<int>
                {
                    LabelText = "粒子数",
                    TransferValueOnCommit = true,
                    Current = config.GetBindable<int>(MSetting.MvisParticleAmount),
                    KeyboardStep = 1,
                },
                new SettingsEnumDropdown<MvisBarType>
                {
                    LabelText = "频谱类型",
                    Current = config.GetBindable<MvisBarType>(MSetting.MvisBarType)
                },
                new SettingsSlider<int>
                {
                    LabelText = "分段数",
                    Current = config.GetBindable<int>(MSetting.MvisVisualizerAmount),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "宽度",
                    Current = config.GetBindable<double>(MSetting.MvisBarWidth),
                    KeyboardStep = 0.1f
                },
                new SettingsSlider<int>
                {
                    LabelText = "每个分段的频谱密度",
                    Current = config.GetBindable<int>(MSetting.MvisBarsPerVisual),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<int>
                {
                    LabelText = "频谱旋转角度",
                    KeyboardStep = 1,
                    Current = config.GetBindable<int>(MSetting.MvisRotation)
                },
                customColourCheckbox = new SettingsCheckbox
                {
                    LabelText = "使用自定义颜色",
                    Current = config.GetBindable<bool>(MSetting.MvisUseCustomColour)
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
                                Current = config.GetBindable<int>(MSetting.MvisRed)
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "绿",
                                KeyboardStep = 1,
                                Current = config.GetBindable<int>(MSetting.MvisGreen)
                            },
                            new SettingsSlider<int>
                            {
                                KeyboardStep = 1,
                                LabelText = "蓝",
                                Current = config.GetBindable<int>(MSetting.MvisBlue)
                            }
                        }
                    }
                },
                new SettingsCheckbox
                {
                    LabelText = "使用lazer自带频谱效果",
                    Current = config.GetBindable<bool>(MSetting.MvisUseOsuLogoVisualisation),
                },
                new SettingsCheckbox
                {
                    LabelText = "禁用Mvis面板",
                    Current = config.GetBindable<bool>(MSetting.MvisDisableRulesetPanel)
                }
            };
        }

        protected override void LoadComplete()
        {
            customColourCheckbox.Current.BindValueChanged(useCustom =>
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
