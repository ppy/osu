using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Objects;
using osuTK;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class RulesetPanel : MvisPlugin
    {
        public RulesetPanel()
        {
            Name = "Mvis面板(Mf-osu自带插件)";
            Description = "用于提供Mvis面板功能(中心的谱面图及周围的粒子效果)";

            Flags.Add(PluginFlags.CanDisable);

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.8f);
        }

        [Resolved]
        private MConfigManager config { get; set; }

        [Resolved]
        private MvisPluginManager manager { get; set; }

        private readonly Bindable<bool> showParticles = new BindableBool();

        private Container particles;

        private BeatmapLogo beatmapLogo;

        private Bindable<bool> disablePanel;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisShowParticles, showParticles);
            disablePanel = config.GetBindable<bool>(MSetting.MvisDisableRulesetPanel);
            Disabled.BindTo(disablePanel);
        }

        protected override void LoadComplete()
        {
            disablePanel.BindValueChanged(v =>
            {
                if (v.NewValue)
                    manager.DisablePlugin(this);
                else
                    manager.ActivePlugin(this);
            }, true);

            base.LoadComplete();
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

            disablePanel.Value = true;
            return base.Disable();
        }

        public override bool Enable()
        {
            this.FadeIn(300).ScaleTo(1, 400, Easing.OutQuint);
            beatmapLogo?.ResponseOnBeatmapChanges();

            disablePanel.Value = false;
            return base.Enable();
        }

        public override void UnLoad()
        {
            disablePanel.UnbindAll();
            base.UnLoad();
        }
    }
}
