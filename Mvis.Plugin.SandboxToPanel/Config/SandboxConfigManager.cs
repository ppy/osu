using System.ComponentModel;
using osu.Framework.Platform;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.Sandbox.Config
{
    public class SandboxConfigManager : PluginConfigManager<SandboxSetting>
    {
        public SandboxConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            //上游
            // Visualizer
            SetDefault(SandboxSetting.ShowParticles, true);
            SetDefault(SandboxSetting.ParticleCount, 500, 50, 1000);
            SetDefault(SandboxSetting.ShowStoryboard, false);
            SetDefault(SandboxSetting.VisualizerLayout, VisualizerLayout.TypeA);
            SetDefault(SandboxSetting.ShowSettingsTip, true);

            // TypeA settings
            SetDefault(SandboxSetting.Radius, 350, 200, 500);
            SetDefault(SandboxSetting.CircularBarType, CircularBarType.Basic);
            SetDefault(SandboxSetting.Rotation, 0, 0, 360);
            SetDefault(SandboxSetting.DecayA, 200, 100, 500);
            SetDefault(SandboxSetting.MultiplierA, 400, 200, 500);
            SetDefault(SandboxSetting.Symmetry, false);
            SetDefault(SandboxSetting.SmoothnessA, 1, 0, 50);
            SetDefault(SandboxSetting.BarWidthA, 3.0, 1, 20);
            SetDefault(SandboxSetting.BarsPerVisual, 120, 10, 3500);
            SetDefault(SandboxSetting.VisualizerAmount, 3, 1, 10);

            // TypeB settings
            SetDefault(SandboxSetting.DecayB, 200, 100, 500);
            SetDefault(SandboxSetting.MultiplierB, 400, 200, 500);
            SetDefault(SandboxSetting.SmoothnessB, 1, 0, 50);
            SetDefault(SandboxSetting.BarWidthB, 3.0, 1, 20);
            SetDefault(SandboxSetting.BarCountB, 120, 10, 3500);
            SetDefault(SandboxSetting.LinearBarType, LinearBarType.Basic);

            //插件
            SetDefault(SandboxSetting.EnableRulesetPanel, true);
            SetDefault(SandboxSetting.IdleAlpha, 1f, 0, 1f);
            SetDefault(SandboxSetting.ShowBeatmapInfo, true);
        }

        protected override string ConfigName => "RulesetPanel";
    }

    public enum SandboxSetting
    {
        //插件
        EnableRulesetPanel,
        IdleAlpha,

        //Type B
        ShowBeatmapInfo,

        //上游
        // Visualizer
        ShowParticles,
        ParticleCount,
        ShowStoryboard,
        VisualizerLayout,
        ShowSettingsTip,

        // TypeA settings
        Radius,
        CircularBarType,
        Rotation,
        DecayA,
        MultiplierA,
        Symmetry,
        SmoothnessA,
        BarWidthA,
        BarsPerVisual,
        VisualizerAmount,

        // TypeB settings
        DecayB,
        MultiplierB,
        SmoothnessB,
        BarWidthB,
        BarCountB,
        LinearBarType
    }

    //上游
    public enum VisualizerLayout
    {
        [Description("类型A")]
        TypeA,

        [Description("类型B")]
        TypeB,

        [Description("无")]
        Empty
    }

    public enum CircularBarType
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

    public enum LinearBarType
    {
        [Description("基本")]
        Basic,

        [Description("圆角")]
        Rounded
    }
}
