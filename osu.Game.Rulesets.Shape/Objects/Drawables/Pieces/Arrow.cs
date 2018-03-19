using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Shape.Objects.Drawables.Pieces
{
    //The cat in the hat is the creepiest fucker you will ever meet
    public class Arrow : Container
    {
        private readonly BaseShape shape;
        private Container line1;
        private Container line1Glow;
        private Container line2;

        public Arrow(BaseShape Shape)
        {
            shape = Shape;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Children = new Drawable[]
            {
                line1 = new Container
                {
                    Rotation = 360 - 45,
                    Size = new Vector2(shape.ShapeSize / 6 , shape.ShapeSize),
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Depth = -2,
                    Colour = Color4.White,
                    Children = new[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Alpha = 1,
                            RelativeSizeAxes = Axes.Both
                        },
                    },
                },
                line2 = new Container
                {
                    Rotation = 45,
                    Size = new Vector2(shape.ShapeSize / 6 , shape.ShapeSize),
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Depth = -1,
                    Colour = Color4.White,
                    Children = new[]
                    {
                        new Box
                        {
                            AlwaysPresent = true,
                            Alpha = 1,
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
                },
                line1Glow = new Container
                {
                    Rotation = 360 - 45,
                    Size = new Vector2(shape.ShapeSize / 6 , shape.ShapeSize),
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Depth = 0,
                    Colour = Color4.White,
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
                },
            };
        }
    }
}
