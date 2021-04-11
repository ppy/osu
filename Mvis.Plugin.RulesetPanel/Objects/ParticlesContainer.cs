using System.Collections.Generic;
using System.Threading;
using Mvis.Plugin.RulesetPanel.Config;
using Mvis.Plugin.RulesetPanel.Objects.Helpers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace Mvis.Plugin.RulesetPanel.Objects
{
    public abstract class ParticlesContainer : CurrentRateContainer
    {
        [Resolved]
        private RulesetPanelConfigManager config { get; set; }

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
            config.BindWith(RulesetPanelSetting.ParticleAmount, countBindable);
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
