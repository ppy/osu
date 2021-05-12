using System.ComponentModel;
using osu.Framework.Platform;
using osu.Game.Screens.Mvis.Plugins.Config;

namespace Mvis.Plugin.RulesetPanel.Config
{
    public class RulesetPanelConfigManager : PluginConfigManager<RulesetPanelSetting>
    {
        public RulesetPanelConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            //上游
            SetDefault(RulesetPanelSetting.ShowParticles, true);
            SetDefault(RulesetPanelSetting.ParticlesCount, 300, 50, 1000);
            SetDefault(RulesetPanelSetting.BarType, BarType.Rounded);
            SetDefault(RulesetPanelSetting.VisualizerAmount, 3, 1, 10);
            SetDefault(RulesetPanelSetting.BarWidth, 3.0, 1, 20);
            SetDefault(RulesetPanelSetting.BarsPerVisual, 120, 1, 3500);
            SetDefault(RulesetPanelSetting.Rotation, 0, 0, 359);
            SetDefault(RulesetPanelSetting.UseCustomColour, false);
            SetDefault(RulesetPanelSetting.Red, 0, 0, 255);
            SetDefault(RulesetPanelSetting.Green, 0, 0, 255);
            SetDefault(RulesetPanelSetting.Blue, 0, 0, 255);
            SetDefault(RulesetPanelSetting.Decay, 200, 100, 500);
            SetDefault(RulesetPanelSetting.Multiplier, 400, 200, 500);
            SetDefault(RulesetPanelSetting.Radius, 350, 100, 450);
            SetDefault(RulesetPanelSetting.LogoPositionX, 0.5f, 0, 1);
            SetDefault(RulesetPanelSetting.LogoPositionY, 0.5f, 0, 1);
            SetDefault(RulesetPanelSetting.Symmetry, false);

            //插件
            SetDefault(RulesetPanelSetting.EnableRulesetPanel, true);
            SetDefault(RulesetPanelSetting.IdleAlpha, 1f, 0, 1f);
        }

        protected override string ConfigName => "RulesetPanel";
    }

    public enum RulesetPanelSetting
    {
        //插件
        EnableRulesetPanel,
        IdleAlpha,

        //上游
        ShowParticles,
        ParticlesCount,
        VisualizerAmount,
        BarWidth,
        BarsPerVisual,
        BarType,
        Rotation,
        UseCustomColour,
        Red,
        Green,
        Blue,
        Decay,
        Multiplier,
        Radius,
        LogoPositionX,
        LogoPositionY,
        Symmetry
    }

    public enum BarType
    {
        [Description("基本")]
        Basic,

        [Description("圆角")]
        Rounded,

        [Description("打砖块")]
        Fall,

        [Description("点状")]
        Dots
    }
}
