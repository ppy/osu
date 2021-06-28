using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osuTK;

namespace Mvis.Plugin.Sandbox.UI
{
    public class SandboxSettings : PluginSettingsSubSection
    {
        private FillFlowContainer typeASettings;
        private FillFlowContainer typeBSettings;

        private readonly Bindable<VisualizerLayout> layoutType = new Bindable<VisualizerLayout>();

        public SandboxSettings(MvisPlugin plugin)
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
                    LabelText = "启用Mvis面板",
                    Current = config.GetBindable<bool>(SandboxSetting.EnableRulesetPanel)
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时面板的不透明度",
                    Current = config.GetBindable<float>(SandboxSetting.IdleAlpha),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsCheckbox
                {
                    LabelText = "显示粒子",
                    Current = config.GetBindable<bool>(SandboxSetting.ShowParticles)
                },
                new SettingsSlider<int>
                {
                    LabelText = "粒子数",
                    TransferValueOnCommit = true,
                    Current = config.GetBindable<int>(SandboxSetting.ParticleCount),
                    KeyboardStep = 1,
                },
                new SettingsEnumDropdown<VisualizerLayout>
                {
                    LabelText = "界面类型",
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
                            LabelText = "半径",
                            KeyboardStep = 1,
                            Current = config.GetBindable<int>(SandboxSetting.Radius)
                        },
                        new SettingsEnumDropdown<CircularBarType>
                        {
                            LabelText = "频谱类型",
                            Current = config.GetBindable<CircularBarType>(SandboxSetting.CircularBarType)
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "旋转角度",
                            KeyboardStep = 1,
                            Current = config.GetBindable<int>(SandboxSetting.Rotation)
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "复原时间",
                            Current = config.GetBindable<int>(SandboxSetting.DecayA),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "高度倍率",
                            Current = config.GetBindable<int>(SandboxSetting.MultiplierA),
                            KeyboardStep = 1
                        },
                        new SettingsCheckbox
                        {
                            LabelText = "对称",
                            Current = config.GetBindable<bool>(SandboxSetting.Symmetry)
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "平滑度",
                            Current = config.GetBindable<int>(SandboxSetting.SmoothnessA),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = "频谱宽度",
                            Current = config.GetBindable<double>(SandboxSetting.BarWidthA),
                            KeyboardStep = 0.1f
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "分段数",
                            Current = config.GetBindable<int>(SandboxSetting.VisualizerAmount),
                            KeyboardStep = 1,
                            TransferValueOnCommit = true
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "频谱密度",
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
                            LabelText = "复原时间",
                            Current = config.GetBindable<int>(SandboxSetting.DecayB),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "高度倍率",
                            Current = config.GetBindable<int>(SandboxSetting.MultiplierB),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "平滑度",
                            Current = config.GetBindable<int>(SandboxSetting.SmoothnessB),
                            KeyboardStep = 1
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = "频谱宽度",
                            Current = config.GetBindable<double>(SandboxSetting.BarWidthB),
                            KeyboardStep = 0.1f
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "频谱数量",
                            Current = config.GetBindable<int>(SandboxSetting.BarCountB),
                            KeyboardStep = 0.1f
                        },
                        new SettingsEnumDropdown<LinearBarType>
                        {
                            LabelText = "频谱类型",
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
