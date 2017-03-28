using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    /// <summary>
    /// A circle piece used for centre hits.
    /// </summary>
    public class CentreHitCirclePiece : Container
    {
        private CirclePiece circle;

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
