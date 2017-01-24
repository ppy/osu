using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    class SpinnerProgressSegment : Container
    {
        private EdgeEffect glow = new EdgeEffect
        {
            Type = EdgeEffectType.Glow,
            Colour = new Color4(68, 170, 221, 200),
            Radius = 0,
            Roundness = 0,
        };
        public SpinnerProgressSegment()
        {
            Size = new Vector2(209f);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 1f;
            Children = new Drawable[]
            {
                new Container
                {
                    Shear = new Vector2(0,0),
                    Masking = true,
                    Size = new Vector2(9,3),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    CornerRadius = 1.5f,
                    EdgeEffect = glow,
                    Children = new[]
                    {
                        new Box
                        {
                            Colour = Color4.White,
                            Alpha = 1f,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                        }
                    }
                }
            };
        }
    }
}
