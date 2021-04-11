using Mvis.Plugin.RulesetPanel.Config;
using Mvis.Plugin.RulesetPanel.Objects;
using Mvis.Plugin.RulesetPanel.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;

namespace Mvis.Plugin.RulesetPanel
{
    public class RulesetPanel : BindableControlledPlugin
    {
        public override TargetLayer Target => TargetLayer.Foreground;

        public RulesetPanel()
        {
            Name = "Mvis面板";
            Description = "用于提供Mvis面板功能(中心的谱面图及周围的粒子效果)";
            Author = "mf-osu; EVAST9919";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload,
                PluginFlags.HasConfig
            });

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.8f);
        }

        private readonly Bindable<bool> showParticles = new BindableBool();
        private readonly BindableFloat idleAlpha = new BindableFloat();

        private Container particles;

        private BeatmapLogo beatmapLogo;

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (RulesetPanelConfigManager)Dependencies.Get<MvisPluginManager>().GetConfigManager(this);
            DependenciesContainer.Cache(config);

            config.BindWith(RulesetPanelSetting.ShowParticles, showParticles);
            config.BindWith(RulesetPanelSetting.EnableRulesetPanel, Value);
            config.BindWith(RulesetPanelSetting.IdleAlpha, idleAlpha);
            idleAlpha.BindValueChanged(onIdleAlphaChanged);

            if (MvisScreen != null)
            {
                MvisScreen.OnIdle += () => idleAlpha.TriggerChange();
                MvisScreen.OnResumeFromIdle += () =>
                {
                    if (Value.Value)
                        this.FadeTo(1, 750, Easing.OutQuint);
                };
            }
        }

        private void onIdleAlphaChanged(ValueChangedEvent<float> v)
        {
            if ((MvisScreen?.OverlaysHidden ?? true) && Value.Value)
                this.FadeTo(v.NewValue, 750, Easing.OutQuint);
        }

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new RulesetPanelConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new RulesetPanelSettings(this);

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                particles = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                new ParallaxContainer
                {
                    ParallaxAmount = -0.0025f,
                    Child = beatmapLogo = new BeatmapLogo
                    {
                        Anchor = Anchor.Centre,
                    }
                }
            }
        };

        protected override bool OnContentLoaded(Drawable content)
        {
            showParticles.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case true:
                        particles.Child = new SpaceParticlesContainer();
                        break;

                    case false:
                        particles.Clear();
                        break;
                }
            }, true);

            if (MvisScreen != null)
            {
                MvisScreen.OnScreenExiting += beatmapLogo.StopResponseOnBeatmapChanges;
                MvisScreen.OnScreenSuspending += beatmapLogo.StopResponseOnBeatmapChanges;
                MvisScreen.OnScreenResuming += () =>
                {
                    if (!Disabled.Value) beatmapLogo.ResponseOnBeatmapChanges();
                };
            }

            return true;
        }

        protected override bool PostInit() => true;

        public override bool Disable()
        {
            this.FadeOut(300, Easing.OutQuint).ScaleTo(0.8f, 400, Easing.OutQuint);
            beatmapLogo?.StopResponseOnBeatmapChanges();

            return base.Disable();
        }

        public override bool Enable()
        {
            this.FadeTo(MvisScreen?.OverlaysHidden ?? false ? idleAlpha.Value : 1, 300).ScaleTo(1, 400, Easing.OutQuint);

            beatmapLogo?.ResponseOnBeatmapChanges();

            return base.Enable();
        }

        public override void UnLoad()
        {
            Value.UnbindAll();
            Disable();

            //bug: 直接调用Expire会导致面板直接消失
            this.Delay(400).Expire();
        }
    }
}
