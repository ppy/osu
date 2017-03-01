using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    public class DrumRollFinisherBodyPiece : DrumRollBodyPiece
    {
        public override float CornerRadius => base.CornerRadius * 1.5f;

        public DrumRollFinisherBodyPiece(float baseLength)
            : base(baseLength)
        {
        }
    }

    public class DrumRollBodyPiece : Container
    {
        private static Color4 yellow_colour = new Color4(238, 170, 0, 255);

        public override float CornerRadius => 32;

        public Container<DrawableDrumRollTick> Ticks;

        public float Progress;

        private float baseLength;

        public DrumRollBodyPiece(float baseLength)
        {
            this.baseLength = baseLength;

            Origin = Anchor.CentreLeft;
            Anchor = Anchor.CentreLeft;

            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 4;

            Position = new Vector2(-CornerRadius, 0);

            EdgeEffect = new EdgeEffect()
            {
                Colour = new Color4(yellow_colour.R, yellow_colour.G, yellow_colour.B, 0.75f),
                Radius = 50,
                Type = EdgeEffectType.Glow,
            };

            Children = new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = yellow_colour
                },
                new DrumRollTrianglesPiece()
                {
                    Size = new Vector2(baseLength + CornerRadius * 2, CornerRadius * 2),

                    Colour = Color4.Black,
                    Alpha = 0.05f,
                },
                new Container()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,

                    Size = new Vector2(baseLength + CornerRadius * 2 - BorderThickness, CornerRadius * 2),
                    Position = new Vector2(BorderThickness / 2f, 0),

                    Masking = true,
                    CornerRadius = CornerRadius,

                    Children = new[]
                    {
                        Ticks = new Container<DrawableDrumRollTick>()
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,

                            Size = new Vector2(baseLength, CornerRadius * 2),
                            Position = new Vector2(CornerRadius, 0)
                        }
                    }
                }
            };
        }

        protected override void Update()
        {
            Size = new Vector2(baseLength * MathHelper.Clamp(1.0f - Progress, 0, 1) + CornerRadius * 2, CornerRadius * 2);
        }
    }
}
