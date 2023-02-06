using M.Resources.Localisation.LLin;
using M.Resources.Localisation.LLin.Plugins;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.LLin.Plugins.Types.SettingsItems;
using osuTK;

#nullable disable

namespace Mvis.Plugin.Sandbox
{
    [Cached]
    public partial class SandboxPlugin : BindableControlledPlugin
    {
        public override TargetLayer Target => TargetLayer.Foreground;
        public override int Version => 10;
        public Bindable<WorkingBeatmap> CurrentBeatmap = new Bindable<WorkingBeatmap>();

        public SandboxPlugin()
        {
            Name = "Sandbox";
            Description = "可能是最好的osu!音乐可视化";
            Author = "EVAST9919; mf-osu";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.8f);
        }

        private readonly BindableFloat idleAlpha = new BindableFloat();

        [BackgroundDependencyLoader]
        private void load()
        {
            idleAlpha.BindValueChanged(onIdleAlphaChanged);

            var config = (SandboxRulesetConfigManager)Dependencies.Get<LLinPluginManager>().GetConfigManager(this);

            config.BindWith(SandboxRulesetSetting.EnableRulesetPanel, Enabled);
            config.BindWith(SandboxRulesetSetting.IdleAlpha, idleAlpha);

            if (LLin != null)
            {
                LLin.OnIdle += () => idleAlpha.TriggerChange();
                LLin.OnActive += () =>
                {
                    if (Enabled.Value)
                        this.FadeTo(1, 750, Easing.OutQuint);

                    CurrentBeatmap.Disabled = false;
                    LLin?.OnBeatmapChanged(onBeatmapChanged, this, true);
                };
            }
        }

        private void onIdleAlphaChanged(ValueChangedEvent<float> v)
        {
            if ((LLin?.InterfacesHidden ?? true) && Enabled.Value)
            {
                this.FadeTo(v.NewValue, 750, Easing.OutQuint);
                if (v.NewValue == 0) CurrentBeatmap.Disabled = true;
            }
        }

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new SandboxRulesetConfigManager(storage);

        private SettingsEntry[] entries;

        public override SettingsEntry[] GetSettingEntries(IPluginConfigManager pluginConfigManager)
        {
            var config = (SandboxRulesetConfigManager)pluginConfigManager;

            entries ??= new SettingsEntry[]
            {
                new BooleanSettingsEntry
                {
                    Name = LLinGenericStrings.EnablePlugin,
                    Bindable = config.GetBindable<bool>(SandboxRulesetSetting.EnableRulesetPanel)
                },
                new NumberSettingsEntry<float>
                {
                    Name = StpStrings.AlphaOnIdle,
                    Bindable = config.GetBindable<float>(SandboxRulesetSetting.IdleAlpha),
                    DisplayAsPercentage = true
                },
                new BooleanSettingsEntry
                {
                    Name = StpStrings.ShowParticles,
                    Bindable = config.GetBindable<bool>(SandboxRulesetSetting.ShowParticles)
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.ParticleCount,
                    //////////TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.ParticleCount),
                    KeyboardStep = 1,
                },
                new EnumSettingsEntry<VisualizerLayout>
                {
                    Name = StpStrings.VisualizerLayoutType,
                    Bindable = config.GetBindable<VisualizerLayout>(SandboxRulesetSetting.VisualizerLayout)
                },
                new SeparatorSettingsEntry
                {
                    Name = "Type A设置"
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.Radius,
                    KeyboardStep = 1,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.Radius)
                },
                new EnumSettingsEntry<CircularBarType>
                {
                    Name = StpStrings.BarType,
                    Bindable = config.GetBindable<CircularBarType>(SandboxRulesetSetting.CircularBarType)
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.Rotation,
                    KeyboardStep = 1,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.Rotation)
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.DecayTime,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.DecayA),
                    KeyboardStep = 1
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.HeightMultiplier,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.MultiplierA),
                    KeyboardStep = 1
                },
                new BooleanSettingsEntry
                {
                    Name = StpStrings.Symmetry,
                    Bindable = config.GetBindable<bool>(SandboxRulesetSetting.Symmetry)
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.Smoothness,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.SmoothnessA),
                    KeyboardStep = 1
                },
                new NumberSettingsEntry<double>
                {
                    Name = StpStrings.BarWidth,
                    Bindable = config.GetBindable<double>(SandboxRulesetSetting.BarWidthA),
                    KeyboardStep = 0.1f
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.VisualizerAmount,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.VisualizerAmount),
                    KeyboardStep = 1,
                    ////////TransferValueOnCommit = true
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.BarsPerVisual,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.BarsPerVisual),
                    KeyboardStep = 1,
                    ////////TransferValueOnCommit = true
                },
                new SeparatorSettingsEntry
                {
                    Name = "Type B设置"
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.DecayTime,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.DecayB),
                    KeyboardStep = 1
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.HeightMultiplier,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.MultiplierB),
                    KeyboardStep = 1
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.Smoothness,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.SmoothnessB),
                    KeyboardStep = 1
                },
                new NumberSettingsEntry<double>
                {
                    Name = StpStrings.BarWidth,
                    Bindable = config.GetBindable<double>(SandboxRulesetSetting.BarWidthB)
                },
                new NumberSettingsEntry<int>
                {
                    Name = StpStrings.BarCount,
                    Bindable = config.GetBindable<int>(SandboxRulesetSetting.BarCountB)
                },
                new EnumSettingsEntry<LinearBarType>
                {
                    Name = StpStrings.BarType,
                    Bindable = config.GetBindable<LinearBarType>(SandboxRulesetSetting.LinearBarType)
                },
            };

            return entries;
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Particles(),
                new LayoutController()
            }
        };

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public override bool Disable()
        {
            this.FadeOut(300, Easing.OutQuint).ScaleTo(0.8f, 400, Easing.OutQuint);

            return base.Disable();
        }

        public override bool Enable()
        {
            bool result = base.Enable();

            this.FadeTo(LLin?.InterfacesHidden ?? false ? idleAlpha.Value : 1, 300).ScaleTo(1, 400, Easing.OutQuint);
            LLin?.OnBeatmapChanged(onBeatmapChanged, this, true);

            return result;
        }

        private void onBeatmapChanged(WorkingBeatmap working)
        {
            if (Disabled.Value || CurrentBeatmap.Disabled) return;

            CurrentBeatmap.Value = working;
        }

        public override void UnLoad()
        {
            if (ContentLoaded)
            {
                //MvisScreen.OnScreenExiting -= beatmapLogo.StopResponseOnBeatmapChanges;
                //MvisScreen.OnScreenSuspending -= beatmapLogo.StopResponseOnBeatmapChanges;
            }

            Enabled.UnbindAll();
            Disable();

            //bug: 直接调用Expire会导致面板直接消失
            this.Delay(400).Expire();
        }
    }
}
