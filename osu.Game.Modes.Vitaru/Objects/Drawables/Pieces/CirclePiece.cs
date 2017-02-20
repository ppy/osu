using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Modes.Vitaru.Objects.Drawables.Pieces
{
    class CirclePiece : Container
    {
        private CircularContainer circleContainer;
        private Container boxContainer;
        private Box box;

        public Color4 CircleColor
        {
            set
            {
                box.Colour = value;
                boxContainer.BorderColour = value;
                circleContainer.EdgeEffect = EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = value.Opacity(0.4f),
                    Radius = CircleWidth / 8,
                };
            }

            get
            {
                return box.Colour;
            }
        }

        public float CircleWidth
        {
            set
            {
                boxContainer.BorderThickness = value / 4;
                boxContainer.CornerRadius = value / 2;
                box.ResizeTo(new Vector2(value));
                circleContainer.ResizeTo(new Vector2(value));
                circleContainer.EdgeEffect = EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = CircleColor.Opacity(0.4f),
                    Radius = value / 8,
                };
            }

            get
            {
                return box.Width;
            }
        }

        public CirclePiece()
        {
            Children = new Drawable[]
            {
                boxContainer = new Container
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    BorderThickness = 4,
                    Depth = 1,
                    BorderColour = Color4.Cyan,
                    Alpha = 1f,
                    CornerRadius = 8,
                    Children = new[]
                    {
                        box = new Box
                        {
                            Colour = Color4.White,
                            Alpha = 1,
                            Size = new Vector2(16),
                        },
                    },
                },
                circleContainer = new CircularContainer
                {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(16),
                        Depth = 2,
                        Masking = true,
                        EdgeEffect = new EdgeEffect
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Cyan.Opacity(0.4f),
                            Radius = 2,
                        }
                }
            };
        }
    }
}