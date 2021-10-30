using System.Collections.Generic;
using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.SideBar.Settings.Items;

namespace Mvis.Plugin.Sandbox.UI
{
    public class RulesetPanelSidebarSection : PluginSidebarSettingsSection
    {
        public RulesetPanelSidebarSection(LLinPlugin plugin)
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
                    Description = LLinGenericStrings.EnablePlugin,
                    Bindable = config.GetBindable<bool>(SandboxSetting.EnableRulesetPanel)
                },
                new SettingsSliderPiece<float>
                {
                    Description = StpStrings.AlphaOnIdle,
                    Bindable = config.GetBindable<float>(SandboxSetting.IdleAlpha),
                    DisplayAsPercentage = true
                },
                new SettingsTogglePiece
                {
                    Description = StpStrings.ShowParticles,
                    Bindable = config.GetBindable<bool>(SandboxSetting.ShowParticles)
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.ParticleCount,
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(SandboxSetting.ParticleCount)
                },
                new SettingsEnumPiece<VisualizerLayout>
                {
                    Description = StpStrings.VisualizerLayoutType,
                    Bindable = layoutType
                },
                new SettingsTogglePiece
                {
                    Description = StpStrings.ShowBeatmapInfo,
                    Bindable = config.GetBindable<bool>(SandboxSetting.ShowBeatmapInfo)
                },
            });

            //workaround: PluginSidebarSettingsSection的grid

            typeAItems.AddRange(new SettingsPieceBasePanel[]
            {
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.Radius,
                    Bindable = config.GetBindable<int>(SandboxSetting.Radius)
                },
                new SettingsEnumPiece<CircularBarType>
                {
                    Description = StpStrings.BarType,
                    Bindable = config.GetBindable<CircularBarType>(SandboxSetting.CircularBarType)
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.Rotation,
                    Bindable = config.GetBindable<int>(SandboxSetting.Rotation)
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.DecayTime,
                    Bindable = config.GetBindable<int>(SandboxSetting.DecayA)
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.HeightMultiplier,
                    Bindable = config.GetBindable<int>(SandboxSetting.MultiplierA)
                },
                new SettingsTogglePiece
                {
                    Description = StpStrings.Symmetry,
                    Bindable = config.GetBindable<bool>(SandboxSetting.Symmetry)
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.Smoothness,
                    Bindable = config.GetBindable<int>(SandboxSetting.SmoothnessA)
                },
                new SettingsSliderPiece<double>
                {
                    Description = StpStrings.BarWidth,
                    Bindable = config.GetBindable<double>(SandboxSetting.BarWidthA)
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.VisualizerAmount,
                    Bindable = config.GetBindable<int>(SandboxSetting.VisualizerAmount),
                    TransferValueOnCommit = true
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.BarsPerVisual,
                    Bindable = config.GetBindable<int>(SandboxSetting.BarsPerVisual),
                    TransferValueOnCommit = true
                }
            });

            typeBItems.AddRange(new SettingsPieceBasePanel[]
            {
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.DecayTime,
                    Bindable = config.GetBindable<int>(SandboxSetting.DecayB),
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.HeightMultiplier,
                    Bindable = config.GetBindable<int>(SandboxSetting.MultiplierB),
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.Smoothness,
                    Bindable = config.GetBindable<int>(SandboxSetting.SmoothnessB),
                },
                new SettingsSliderPiece<double>
                {
                    Description = StpStrings.BarWidth,
                    Bindable = config.GetBindable<double>(SandboxSetting.BarWidthB),
                },
                new SettingsSliderPiece<int>
                {
                    Description = StpStrings.BarCount,
                    Bindable = config.GetBindable<int>(SandboxSetting.BarCountB),
                },
                new SettingsEnumPiece<LinearBarType>
                {
                    Description = StpStrings.BarType,
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
