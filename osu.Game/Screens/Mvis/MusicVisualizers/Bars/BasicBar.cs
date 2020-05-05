using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.UI.Objects.MusicVisualizers.Bars
{
    public class BasicBar : Container
    {
        public BasicBar()
        {
            Child = CreateContent();
        }

        protected virtual Drawable CreateContent() => new Box
        {
            EdgeSmoothness = Vector2.One,
            RelativeSizeAxes = Axes.Both,
            Colour = Color4.White,
        };

        public virtual void SetValue(float amplitudeValue, float valueMultiplier, int softness)
        {
            var newHeight = ValueFormula(amplitudeValue, valueMultiplier);

            // Don't allow resize if new height less than current
            if (newHeight <= Height)
                return;

            this.ResizeHeightTo(newHeight).Then().ResizeHeightTo(0, softness);
        }

        protected virtual float ValueFormula(float amplitudeValue, float valueMultiplier) => amplitudeValue * valueMultiplier;
    }
}
