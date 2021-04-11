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
            SetDefault(RulesetPanelSetting.EnableRulesetPanel, true);
            SetDefault(RulesetPanelSetting.ParticleAmount, 350, 0, 350);
            SetDefault(RulesetPanelSetting.ShowParticles, true);
            SetDefault(RulesetPanelSetting.BarType, MvisBarType.Rounded);
            SetDefault(RulesetPanelSetting.VisualizerAmount, 3, 1, 5);
            SetDefault(RulesetPanelSetting.BarWidth, 3.0, 1, 20);
            SetDefault(RulesetPanelSetting.BarsPerVisual, 120, 1, 200);
            SetDefault(RulesetPanelSetting.Rotation, 0, 0, 359);
            SetDefault(RulesetPanelSetting.UseCustomColour, false);
            SetDefault(RulesetPanelSetting.Red, 0, 0, 255);
            SetDefault(RulesetPanelSetting.Green, 0, 0, 255);
            SetDefault(RulesetPanelSetting.Blue, 0, 0, 255);
            SetDefault(RulesetPanelSetting.UseOsuLogoVisualisation, false);
            SetDefault(RulesetPanelSetting.IdleAlpha, 1f, 0, 1f);
        }

        protected override string ConfigName => "StoryboardSupport";
    }

    public enum RulesetPanelSetting
    {
        EnableRulesetPanel,
        ParticleAmount,
        ShowParticles,
        VisualizerAmount,
        BarWidth,
        BarsPerVisual,
        BarType,
        Rotation,
        UseCustomColour,
        Red,
        Green,
        Blue,
        UseOsuLogoVisualisation,
        IdleAlpha
    }

    public enum MvisBarType
    {
        [Description("基本")]
        Basic,

        [Description("圆角")]
        Rounded,

        [Description("打砖块")]
        Fall
    }
}
