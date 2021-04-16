using Mvis.Plugin.RulesetPanel.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osuTK;

namespace Mvis.Plugin.RulesetPanel.UI
{
    public class RulesetPanelSettings : PluginSettingsSubSection
    {
        private Container resizableContainer;
        private SettingsCheckbox customColourCheckbox;

        public RulesetPanelSettings(MvisPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (RulesetPanelConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用Mvis面板",
                    Current = config.GetBindable<bool>(RulesetPanelSetting.EnableRulesetPanel)
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时面板的不透明度",
                    Current = config.GetBindable<float>(RulesetPanelSetting.IdleAlpha),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsCheckbox
                {
                    LabelText = "显示粒子",
                    Current = config.GetBindable<bool>(RulesetPanelSetting.ShowParticles)
                },
                new SettingsSlider<int>
                {
                    LabelText = "粒子数",
                    TransferValueOnCommit = true,
                    Current = config.GetBindable<int>(RulesetPanelSetting.ParticleAmount),
                    KeyboardStep = 1,
                },
                new SettingsEnumDropdown<MvisBarType>
                {
                    LabelText = "频谱类型",
                    Current = config.GetBindable<MvisBarType>(RulesetPanelSetting.BarType)
                },
                new SettingsSlider<int>
                {
                    LabelText = "分段数",
                    Current = config.GetBindable<int>(RulesetPanelSetting.VisualizerAmount),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "宽度",
                    Current = config.GetBindable<double>(RulesetPanelSetting.BarWidth),
                    KeyboardStep = 0.1f
                },
                new SettingsSlider<int>
                {
                    LabelText = "每个分段的频谱密度",
                    Current = config.GetBindable<int>(RulesetPanelSetting.BarsPerVisual),
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<int>
                {
                    LabelText = "频谱旋转角度",
                    KeyboardStep = 1,
                    Current = config.GetBindable<int>(RulesetPanelSetting.Rotation)
                },
                customColourCheckbox = new SettingsCheckbox
                {
                    LabelText = "使用自定义颜色",
                    Current = config.GetBindable<bool>(RulesetPanelSetting.UseCustomColour)
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
                                Current = config.GetBindable<int>(RulesetPanelSetting.Red)
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "绿",
                                KeyboardStep = 1,
                                Current = config.GetBindable<int>(RulesetPanelSetting.Green)
                            },
                            new SettingsSlider<int>
                            {
                                KeyboardStep = 1,
                                LabelText = "蓝",
                                Current = config.GetBindable<int>(RulesetPanelSetting.Blue)
                            }
                        }
                    }
                },
                new SettingsCheckbox
                {
                    LabelText = "使用lazer自带频谱效果",
                    Current = config.GetBindable<bool>(RulesetPanelSetting.UseOsuLogoVisualisation),
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
