using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Shape.Objects.Drawables.Pieces
{
    public class ShapeTriangle : Container
    {
        private BaseShape shape;
        private Triangle triangle;

        public ShapeTriangle(BaseShape Shape)
        {
            shape = Shape;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Children = new Drawable[]
            {
                triangle = new Triangle
                {
                    Size = new Vector2(shape.ShapeSize),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Depth = -2,
                }
            };
        }
    }
}
