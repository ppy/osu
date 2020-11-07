using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class ComponentContainer : Container
    {
        protected virtual float cornerRadius => 25f;
        protected virtual bool masking => true;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = masking;
            CornerRadius = cornerRadius;

            Add(new Box
            {
                Depth = float.MaxValue,
                Colour = colourProvider.Background4,
                RelativeSizeAxes = Axes.Both,
            });
        }
    }
}
