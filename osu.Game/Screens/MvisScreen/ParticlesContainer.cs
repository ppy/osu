using osu.Game.Configuration;
using osu.Framework.Graphics;
using osu.Game.Screens.Mvis.UI.Objects.Helpers;
using osu.Framework.Bindables;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Mvis.UI.Objects
{
    public abstract class ParticlesContainer : CurrentRateContainer
    {
        /// <summary>
        /// Number of milliseconds between addition of a new particle.
        /// </summary>
        private const float time_between_updates = 50;

        private Bindable<int> MvisParticleAmount = new Bindable<int>();

        /// <summary>
        /// Maximum allowed amount of particles which can be shown at once.
        /// </summary>
        private int MaxParticlesCount = 65;

        protected ParticlesContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.MvisParticleAmount, MvisParticleAmount);
            MaxParticlesCount = MvisParticleAmount.Value;

            MvisParticleAmount.ValueChanged += _ => MaxParticlesCount = MvisParticleAmount.Value;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            generateParticles(true);
        }

        private void generateParticles(bool firstLoad)
        {
            var currentParticlesCount = Children.Count;

            if (currentParticlesCount < MaxParticlesCount)
            {
                for (int i = 0; i < MaxParticlesCount - currentParticlesCount; i++)
                    Add(CreateParticle(firstLoad));
            }

            Scheduler.AddDelayed(() => generateParticles(false), time_between_updates);
        }

        protected abstract Drawable CreateParticle(bool firstLoad);
    }
}
