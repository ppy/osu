using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Shape.Objects.Drawables.Pieces
{
    public class ShapeSquare : Container
    {
        private BaseShape shape;
        private Container box;

        public ShapeSquare(BaseShape Shape)
        {
            shape = Shape;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Children = new Drawable[]
            {
                box = new Container
                {
                    Size = new Vector2(shape.ShapeSize),
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = shape.ShapeSize / 6,
                    Depth = -2,
                    BorderColour = Color4.White,
                    CornerRadius = shape.ShapeSize / 8,
                    Children = new[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Hollow = true,
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.White.Opacity(0.25f),
                        Radius = shape.ShapeSize / 4,
                    }
                }
            };
        }
    }
}
