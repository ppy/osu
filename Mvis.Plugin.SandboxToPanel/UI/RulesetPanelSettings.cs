using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osuTK;

namespace Mvis.Plugin.Sandbox.UI
{
    public class SandboxSettings : PluginSettingsSubSection
    {
        private FillFlowContainer typeASettings;
        private FillFlowContainer typeBSettings;

        private readonly Bindable<VisualizerLayout> layoutType = new Bindable<VisualizerLayout>();

        public SandboxSettings(LLinPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SandboxConfigManager)ConfigManager;

            config.BindWith(SandboxSetting.VisualizerLayout, layoutType);

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = LLinGenericStrings.EnablePlugin,
                    Current = config.GetBindable<bool>(SandboxSetting.EnableRulesetPanel)
                },
                new SettingsSlider<float>
                {
                    LabelText = StpStrings.AlphaOnIdle,
                    Current = config.GetBindable<float>(SandboxSetting.IdleAlpha),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsCheckbox
                {
                    LabelText = StpStrings.ShowParticles,
                    Current = config.GetBindable<bool>(SandboxSetting.ShowParticles)
                },
                new SettingsSlider<int>
                {
                    LabelText = StpStrings.ParticleCount,
                    TransferValueOnCommit = true,
                    Current = config.GetBindable<int>(SandboxSetting.ParticleCount),
                    KeyboardStep = 1,
                },
                new SettingsCheckbox
                {
                    LabelText = StpStrings.ShowBeatmapInfo,
                    Current = config.GetBindable<bool>(SandboxSetting.ShowBeatmapInfo)
                },
                new SettingsEnumDropdown<VisualizerLayout>
                {
                    LabelText = StpStrings.VisualizerLayoutType,
                    Current = layoutType
                },
                typeASettings = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    Spacing = new Vector2(0, 8),
                    Children = new Drawable[]
                    {
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.Radius,
                            KeyboardStep = 1,
                            Current = config.GetBindable<int>(SandboxSetting.Radius)
                        },
                        new SettingsEnumDropdown<CircularBarType>
                        {
                            LabelText = StpStrings.BarType,
                            Current = config.GetBindable<CircularBarType>(SandboxSetting.CircularBarType)
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.Rotation,
                            KeyboardStep = 1,
                            Current = config.GetBindable<int>(SandboxSetting.Rotation)
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.DecayTime,
                            Current = config.GetBindable<int>(SandboxSetting.DecayA),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.HeightMultiplier,
                            Current = config.GetBindable<int>(SandboxSetting.MultiplierA),
                            KeyboardStep = 1
                        },
                        new SettingsCheckbox
                        {
                            LabelText = StpStrings.Symmetry,
                            Current = config.GetBindable<bool>(SandboxSetting.Symmetry)
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.Smoothness,
                            Current = config.GetBindable<int>(SandboxSetting.SmoothnessA),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = StpStrings.BarWidth,
                            Current = config.GetBindable<double>(SandboxSetting.BarWidthA),
                            KeyboardStep = 0.1f
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.VisualizerAmount,
                            Current = config.GetBindable<int>(SandboxSetting.VisualizerAmount),
                            KeyboardStep = 1,
                            TransferValueOnCommit = true
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.BarsPerVisual,
                            Current = config.GetBindable<int>(SandboxSetting.BarsPerVisual),
                            KeyboardStep = 1,
                            TransferValueOnCommit = true
                        }
                    }
                },
                typeBSettings = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0, 8),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.DecayTime,
                            Current = config.GetBindable<int>(SandboxSetting.DecayB),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.HeightMultiplier,
                            Current = config.GetBindable<int>(SandboxSetting.MultiplierB),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.Smoothness,
                            Current = config.GetBindable<int>(SandboxSetting.SmoothnessB),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = StpStrings.BarWidth,
                            Current = config.GetBindable<double>(SandboxSetting.BarWidthB),
                            KeyboardStep = 0.1f
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = StpStrings.BarCount,
                            Current = config.GetBindable<int>(SandboxSetting.BarCountB),
                            KeyboardStep = 0.1f
                        },
                        new SettingsEnumDropdown<LinearBarType>
                        {
                            LabelText = StpStrings.BarType,
                            Current = config.GetBindable<LinearBarType>(SandboxSetting.LinearBarType)
                        },
                    }
                },
                //new SettingsSlider<float, PositionSlider>
                //{
                //    LabelText = "水平位置",
                //    KeyboardStep = 0.01f,
                //    Current = config.GetBindable<float>(SandboxSetting.LogoPositionX)
                //},
                //new SettingsSlider<float, PositionSlider>
                //{
                //    LabelText = "竖直位置",
                //    KeyboardStep = 0.01f,
                //    Current = config.GetBindable<float>(SandboxSetting.LogoPositionY)
                //},
            };
        }

        protected override void LoadComplete()
        {
            layoutType.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case VisualizerLayout.Empty:
                        typeASettings.FadeOut();
                        typeBSettings.FadeOut();
                        break;

                    case VisualizerLayout.TypeA:
                        typeASettings.FadeIn();
                        typeBSettings.FadeOut();
                        break;

                    case VisualizerLayout.TypeB:
                        typeASettings.FadeOut();
                        typeBSettings.FadeIn();
                        break;
                }
            }, true);
            base.LoadComplete();
        }
    }
}
