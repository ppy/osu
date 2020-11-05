using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Modules;

namespace osu.Game.Screens.Mvis.UI
{
    public class SongProgressBar : ProgressBar
    {
        [Resolved]
        private CustomColourProvider colourProvider { get; set; }
        private float idleAlpha = 0.5f;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.BottomCentre;
            Anchor = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.X;
            Height = 5;
            Alpha = idleAlpha;

            FillColour = colourProvider.Highlight1;
            BackgroundColour = colourProvider.Light4.Opacity(0.5f);

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                FillColour = colourProvider.Highlight1;
                BackgroundColour = colourProvider.Light4.Opacity(0.5f);
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.ResizeHeightTo(7, 300, Easing.OutQuint).FadeTo(1, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.ResizeHeightTo(5, 300, Easing.OutQuint).FadeTo(idleAlpha, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override void UpdateValue(float value) =>
            fill.ResizeWidthTo(value * UsableWidth, 300, Easing.OutQuint);
    }
}