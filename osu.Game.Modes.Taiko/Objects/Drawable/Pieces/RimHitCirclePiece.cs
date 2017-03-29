// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    public class RimHitCirclePiece : Container
    {
        private readonly CirclePiece circle;

        public RimHitCirclePiece(CirclePiece piece)
        {
            Add(circle = piece);

            circle.Add(new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(CirclePiece.SYMBOL_SIZE),
                BorderThickness = CirclePiece.SYMBOL_BORDER,
                BorderColour = Color4.White,
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            circle.AccentColour = colours.BlueDarker;
        }
    }
}
