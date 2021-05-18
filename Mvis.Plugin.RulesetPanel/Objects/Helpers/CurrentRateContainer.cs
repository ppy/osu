using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace Mvis.Plugin.RulesetPanel.Objects.Helpers
{
    public class CurrentRateContainer : RateAdjustableContainer
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
