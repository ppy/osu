using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.Edit.Blueprints
{
    public class HitPiece : CompositeDrawable
    {
        public HitPiece()
        {
            Size = new Vector2(16);
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativePositionAxes = Axes.Both;

            InternalChild = new Container
            {
                Masking = true,
                BorderThickness = 10,
                BorderColour = Color4.Yellow,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };
        }
    }
}
