using Mvis.Plugin.RulesetPanel.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.SideBar.Settings.Items;

namespace Mvis.Plugin.RulesetPanel.UI
{
    public class RulesetPanelSidebarSection : PluginSidebarSettingsSection
    {
        public RulesetPanelSidebarSection(MvisPlugin plugin)
            : base(plugin)
        {
            Title = "Mvis2Player";
        }

        public override int Columns => 5;

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (RulesetPanelConfigManager)ConfigManager;
            AddRange(new Drawable[]
            {
                new SettingsTogglePiece
                {
                    Description = "启用Mvis面板",
                    Bindable = config.GetBindable<bool>(RulesetPanelSetting.EnableRulesetPanel)
                },
                new SettingsSliderPiece<float>
                {
                    Description = "面板不透明度(空闲)",
                    Bindable = config.GetBindable<float>(RulesetPanelSetting.IdleAlpha),
                    DisplayAsPercentage = true,
                },
                new SettingsTogglePiece
                {
                    Description = "显示粒子",
                    Bindable = config.GetBindable<bool>(RulesetPanelSetting.ShowParticles)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "粒子数",
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.ParticlesCount)
                },
                new SettingsEnumPiece<BarType>
                {
                    Icon = FontAwesome.Solid.WaveSquare,
                    Description = "频谱类型",
                    Bindable = config.GetBindable<BarType>(RulesetPanelSetting.BarType)
                },
                new SettingsTogglePiece
                {
                    Icon = FontAwesome.Solid.AlignCenter,
                    Description = "对称",
                    Bindable = config.GetBindable<bool>(RulesetPanelSetting.Symmetry)
                },
                new SettingsSliderPiece<int>
                {
                    Icon = FontAwesome.Regular.Clock,
                    Description = "复原时间",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.Decay)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "高度倍率",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.Multiplier)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "半径",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.Radius)
                },
                new SettingsSliderPiece<float>
                {
                    Description = "水平位置",
                    Bindable = config.GetBindable<float>(RulesetPanelSetting.LogoPositionX),
                    DisplayAsPercentage = true
                },
                new SettingsSliderPiece<float>
                {
                    Description = "竖直位置",
                    Bindable = config.GetBindable<float>(RulesetPanelSetting.LogoPositionY),
                    DisplayAsPercentage = true
                },
                new SettingsSliderPiece<int>
                {
                    Description = "分段数",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.VisualizerAmount),
                    TransferValueOnCommit = true
                },
                new SettingsSliderPiece<double>
                {
                    Description = "宽度",
                    Bindable = config.GetBindable<double>(RulesetPanelSetting.BarWidth),
                },
                new SettingsSliderPiece<int>
                {
                    Icon = FontAwesome.Solid.Signal,
                    Description = "频谱密度",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.BarsPerVisual),
                    TransferValueOnCommit = true
                },
                new SettingsSliderPiece<int>
                {
                    Icon = FontAwesome.Solid.RedoAlt,
                    Description = "旋转角度",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.Rotation)
                },
                new SettingsTogglePiece
                {
                    Description = "使用自定义颜色",
                    Bindable = config.GetBindable<bool>(RulesetPanelSetting.UseCustomColour)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "红",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.Red)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "绿",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.Green)
                },
                new SettingsSliderPiece<int>
                {
                    Description = "蓝",
                    Bindable = config.GetBindable<int>(RulesetPanelSetting.Blue)
                }
            });
        }
    }
}
