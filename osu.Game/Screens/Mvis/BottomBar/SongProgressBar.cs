using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Mvis.BottomBar
{
    public class SongProgressBar : ProgressBar
    {
        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private const float idle_alpha = 0.5f;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.BottomCentre;
            Anchor = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.X;
            Height = 5;
            Alpha = idle_alpha;

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                FillColour = colourProvider.Highlight1;
                BackgroundColour = colourProvider.Light4.Opacity(0.5f);
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.ResizeHeightTo(7, 300, Easing.OutQuint).FadeTo(1, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.ResizeHeightTo(5, 300, Easing.OutQuint).FadeTo(idle_alpha, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override void UpdateValue(float value) =>
            fill.Width = value * UsableWidth;
    }
}
