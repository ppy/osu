using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Screens.Mvis.UI.Objects.Helpers;
using System.Collections.Generic;
using System.Threading;

namespace osu.Game.Screens.Mvis.UI.Objects
{
    public abstract class ParticlesContainer : CurrentRateContainer
    {
        [Resolved(canBeNull: true)]
        private MfConfigManager config { get; set; }

        private readonly Bindable<int> countBindable = new Bindable<int>(200);

        protected ParticlesContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            config?.BindWith(MfSetting.MvisParticleAmount, countBindable);
            countBindable.BindValueChanged(onCountChanged, true);
        }

        private void onCountChanged(ValueChangedEvent<int> e)
        {
            cancellationToken?.Cancel();
            Clear();
            generateParticles(e.NewValue);
        }

        private CancellationTokenSource cancellationToken;

        private void generateParticles(int count)
        {
            var particles = new List<Drawable>();

            for (int i = 0; i < count; i++)
                particles.Add(CreateParticle());

            LoadComponentsAsync(particles, AddRange, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected abstract Drawable CreateParticle();

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
