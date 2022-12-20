using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.MusicHelpers
{
    public partial class CurrentRateContainer : RateAdjustableContainer
    {
        protected readonly BindableBool IsKiai = new BindableBool();

        protected override Container<Drawable> Content => content;

        private readonly MusicIntensityController intensityController;
        private readonly Container content;

        public CurrentRateContainer()
        {
            AddRangeInternal(new Drawable[]
            {
                content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                intensityController = new MusicIntensityController()
            });

            IsKiai.BindTo(intensityController.IsKiai);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            intensityController.Intensity.BindValueChanged(rate => Rate = rate.NewValue);
        }
    }
}
