using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Objects;

namespace osu.Game.Screens.Mvis.Plugins
{
    public class RulesetPanel : MvisPlugin
    {
        public RulesetPanel()
        {
            Name = "Mvis面板";
            Description = "用于提供Mvis面板功能(中心的谱面图及周围的粒子效果)";

            RelativeSizeAxes = Axes.Both;
        }

        private readonly Bindable<bool> showParticles = new BindableBool();

        private readonly Container particles = new Container
        {
            RelativeSizeAxes = Axes.Both
        };

        private readonly BeatmapLogo beatmapLogo = new BeatmapLogo
        {
            Anchor = Anchor.Centre,
        };

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisShowParticles, showParticles);
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                particles,
                new ParallaxContainer
                {
                    ParallaxAmount = -0.0025f,
                    Child = beatmapLogo
                }
            }
        };

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit()
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
                MvisScreen.OnScreenResuming += beatmapLogo.ResponseOnBeatmapChanges;
            }

            return true;
        }
    }
}
