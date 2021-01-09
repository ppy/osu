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
        protected override string Header => "settings.mvis.visual.header";

        private SettingsCheckbox customColourCheckbox;
        private Container resizableContainer;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.visual.enableBgTriangles",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableBgTriangles),
                },
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.visual.showParticles",
                    Current = config.GetBindable<bool>(MSetting.MvisShowParticles)
                },
                new SettingsSlider<int>
                {
                    LabelText = "settings.mvis.visual.particleAmount",
                    TransferValueOnCommit = true,
                    Current = config.GetBindable<int>(MSetting.MvisParticleAmount),
                    KeyboardStep = 1,
                },
                new SettingsEnumDropdown<MvisBarType>
                {
                    LabelText = "settings.mvis.visual.barType",
                    Current = config.GetBindable<MvisBarType>(MSetting.MvisBarType)
                },
                new SettingsSlider<int>
                {
                    LabelText = "settings.mvis.visual.visualizerAmount",
                    Current = config.GetBindable<int>(MSetting.MvisVisualizerAmount),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "settings.mvis.visual.barWidth",
                    Current = config.GetBindable<double>(MSetting.MvisBarWidth),
                    KeyboardStep = 0.1f
                },
                new SettingsSlider<int>
                {
                    LabelText = "settings.mvis.visual.barsPerVisual",
                    Current = config.GetBindable<int>(MSetting.MvisBarsPerVisual),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<int>
                {
                    LabelText = "settings.mvis.visual.rotation",
                    KeyboardStep = 1,
                    Current = config.GetBindable<int>(MSetting.MvisRotation)
                },
                customColourCheckbox = new SettingsCheckbox
                {
                    LabelText = "settings.mvis.visual.useCustomColor",
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
                                LabelText = "settings.mvis.visual.iR",
                                KeyboardStep = 1,
                                Current = config.GetBindable<int>(MSetting.MvisRed)
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "settings.mvis.visual.iG",
                                KeyboardStep = 1,
                                Current = config.GetBindable<int>(MSetting.MvisGreen)
                            },
                            new SettingsSlider<int>
                            {
                                KeyboardStep = 1,
                                LabelText = "settings.mvis.visual.iB",
                                Current = config.GetBindable<int>(MSetting.MvisBlue)
                            }
                        }
                    }
                },
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.visual.useOsuLogoVisualisation",
                    Current = config.GetBindable<bool>(MSetting.MvisUseOsuLogoVisualisation),
                },
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
