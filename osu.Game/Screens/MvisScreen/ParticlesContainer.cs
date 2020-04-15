using osu.Framework.Graphics;
using osu.Game.Screens.Mvis.UI.Objects.Helpers;

namespace osu.Game.Screens.Mvis.UI.Objects
{
    public abstract class ParticlesContainer : CurrentRateContainer
    {
        /// <summary>
        /// Number of milliseconds between addition of a new particle.
        /// </summary>
        private const float time_between_updates = 50;

        /// <summary>
        /// Maximum allowed amount of particles which can be shown at once.
        /// </summary>
        protected virtual int MaxParticlesCount => 350;

        protected ParticlesContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
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
