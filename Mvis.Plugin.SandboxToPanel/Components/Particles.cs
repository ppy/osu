using Mvis.Plugin.RulesetPanel.Components.MusicHelpers;
using osu.Framework.Graphics;

namespace Mvis.Plugin.RulesetPanel.Components
{
    public class Particles : CurrentRateContainer
    {
        private readonly ParticlesDrawable particles;

        public Particles()
        {
            RelativeSizeAxes = Axes.Both;
            Add(particles = new ParticlesDrawable());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            IsKiai.BindValueChanged(kiai =>
            {
                if (kiai.NewValue)
                {
                    particles.SetRandomDirection();
                }
                else
                    particles.Direction.Value = MoveDirection.Forward;
            });
        }
    }
}
