// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    /// <summary>
    /// A circle piece used for centre hits.
    /// </summary>
    public class CentreHitCirclePiece : Container
    {
        private readonly CirclePiece circle;

        public CentreHitCirclePiece(CirclePiece piece)
        {
            Add(circle = piece);

            circle.Add(new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(CirclePiece.SYMBOL_INNER_SIZE),
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            circle.AccentColour = colours.PinkDarker;
        }
    }
}
