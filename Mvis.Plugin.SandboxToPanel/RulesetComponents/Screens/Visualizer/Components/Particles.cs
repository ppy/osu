using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.MusicHelpers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

#nullable disable

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components
{
    public partial class Particles : CurrentRateContainer
    {
        private readonly Bindable<string> colour = new Bindable<string>("#ffffff");
        private readonly Bindable<ParticlesDirection> direction = new Bindable<ParticlesDirection>();

        private ParticlesDrawable particles;

        [Resolved(canBeNull: true)]
        private SandboxRulesetConfigManager config { get; set; }

        public Particles()
        {
            RelativeSizeAxes = Axes.Both;
            Add(particles = new ParticlesDrawable());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            config?.BindWith(SandboxRulesetSetting.ParticlesColour, colour);
            config?.BindWith(SandboxRulesetSetting.ParticlesDirection, direction);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            direction.BindValueChanged(_ => updateDirection());
            IsKiai.BindValueChanged(_ => updateDirection(), true);

            colour.BindValueChanged(c => particles.Colour = Colour4.FromHex(c.NewValue), true);
        }

        private void updateDirection()
        {
            particles.Direction.Value = direction.Value;
        }
    }
}
