using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Objects;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;

namespace Mvis.Plugin.RulesetPanel
{
    public class RulesetPanel : BindableControlledPlugin
    {
        public override TargetLayer Target => TargetLayer.Foreground;

        public RulesetPanel()
        {
            Name = "Mvis面板(内置)";
            Description = "用于提供Mvis面板功能(中心的谱面图及周围的粒子效果)";
            Author = "mf-osu; EVAST9919";

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

        [Resolved]
        private MConfigManager config { get; set; }

        private readonly Bindable<bool> showParticles = new BindableBool();
        private readonly BindableFloat idleAlpha = new BindableFloat();

        private Container particles;

        private BeatmapLogo beatmapLogo;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisShowParticles, showParticles);
            config.BindWith(MSetting.MvisEnableRulesetPanel, Value);
            config.BindWith(MSetting.MvisContentAlpha, idleAlpha);
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
