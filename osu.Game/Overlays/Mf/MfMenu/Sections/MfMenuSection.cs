using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.MfMenu
{
    public abstract class MfMenuSection : Container
    {
        private OverlayColourProvider colourProvider  = new OverlayColourProvider(OverlayColourScheme.BlueLighter);

        public MfMenuSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour =colourProvider.Background5,
                }
            };
        }
    }
}