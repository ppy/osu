using System.Collections.Generic;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.SideBar.Settings.Items;

namespace Mvis.Plugin.Sandbox.UI
{
    public class RulesetPanelSidebarSection : PluginSidebarSettingsSection
    {
        public RulesetPanelSidebarSection(MvisPlugin plugin)
            : base(plugin)
        {
            Title = "Sandbox";
        }

        public override int Columns => 5;

        private readonly Bindable<VisualizerLayout> layoutType = new Bindable<VisualizerLayout>();

        private readonly List<Drawable> typeAItems = new List<Drawable>();
        private readonly List<Drawable> typeBItems = new List<Drawable>();

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SandboxConfigManager)ConfigManager;

            config.BindWith(SandboxSetting.VisualizerLayout, layoutType);

            AddRange(new Drawable[]
            {
                new SettingsTogglePiece
                {
                    Description = "启用Mvis面板",
                    Bindable = config.GetBindable<bool>(SandboxSetting.EnableRulesetPanel)
                },
                new SettingsSliderPiece<float>
                {
                    Description = "空闲时面板的不透明度",
                    Bindable = config.GetBindable<float>(SandboxSetting.IdleAlpha),
                    DisplayAsPercentage = true
                },
                new SettingsTogglePiece
                {
                    Description = "显示粒子",
                    Bindable = config.GetBindable<bool>(SandboxSetting.ShowParticles)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "粒子数",
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(SandboxSetting.ParticleCount)
                },
                new SettingsEnumPiece<VisualizerLayout>
                {
                    Description = "界面类型",
                    Bindable = layoutType
                }
            });

            //workaround: PluginSidebarSettingsSection的grid

            typeAItems.AddRange(new SettingsPieceBasePanel[]
            {
                new SettingsSliderPiece<int>
                {
                    Description = "半径",
                    Bindable = config.GetBindable<int>(SandboxSetting.Radius)
                },
                new SettingsEnumPiece<CircularBarType>
                {
                    Description = "频谱类型",
                    Bindable = config.GetBindable<CircularBarType>(SandboxSetting.CircularBarType)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "旋转角度",
                    Bindable = config.GetBindable<int>(SandboxSetting.Rotation)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "复原时间",
                    Bindable = config.GetBindable<int>(SandboxSetting.DecayA)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "高度倍率",
                    Bindable = config.GetBindable<int>(SandboxSetting.MultiplierA)
                },
                new SettingsTogglePiece
                {
                    Description = "对称",
                    Bindable = config.GetBindable<bool>(SandboxSetting.Symmetry)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "平滑度",
                    Bindable = config.GetBindable<int>(SandboxSetting.SmoothnessA)
                },
                new SettingsSliderPiece<double>
                {
                    Description = "频谱宽度",
                    Bindable = config.GetBindable<double>(SandboxSetting.BarWidthA)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "分段数",
                    Bindable = config.GetBindable<int>(SandboxSetting.VisualizerAmount),
                    TransferValueOnCommit = true
                },
                new SettingsSliderPiece<int>
                {
                    Description = "频谱密度",
                    Bindable = config.GetBindable<int>(SandboxSetting.BarsPerVisual),
                    TransferValueOnCommit = true
                }
            });

            typeBItems.AddRange(new SettingsPieceBasePanel[]
            {
                new SettingsSliderPiece<int>
                {
                    Description = "复原时间",
                    Bindable = config.GetBindable<int>(SandboxSetting.DecayB),
                },
                new SettingsSliderPiece<int>
                {
                    Description = "高度倍率",
                    Bindable = config.GetBindable<int>(SandboxSetting.MultiplierB),
                },
                new SettingsSliderPiece<int>
                {
                    Description = "平滑度",
                    Bindable = config.GetBindable<int>(SandboxSetting.SmoothnessB),
                },
                new SettingsSliderPiece<double>
                {
                    Description = "频谱宽度",
                    Bindable = config.GetBindable<double>(SandboxSetting.BarWidthB),
                },
                new SettingsSliderPiece<int>
                {
                    Description = "频谱数量",
                    Bindable = config.GetBindable<int>(SandboxSetting.BarCountB),
                },
                new SettingsEnumPiece<LinearBarType>
                {
                    Description = "频谱类型",
                    Bindable = config.GetBindable<LinearBarType>(SandboxSetting.LinearBarType)
                },
            });

            AddRange(typeAItems.ToArray());
            AddRange(typeBItems.ToArray());
        }

        protected override void LoadComplete()
        {
            layoutType.BindValueChanged(v =>
            {
                foreach (var p in typeAItems)
                    p.FadeOut();

                foreach (var p in typeBItems)
                    p.FadeOut();

                switch (v.NewValue)
                {
                    case VisualizerLayout.Empty:
                        break;

                    case VisualizerLayout.TypeA:
                        foreach (var p in typeAItems)
                            p.FadeIn();
                        break;

                    case VisualizerLayout.TypeB:
                        foreach (var p in typeBItems)
                            p.FadeIn();

                        break;
                }
            }, true);
            base.LoadComplete();
        }
    }
}
