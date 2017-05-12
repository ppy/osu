// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables.Pieces
{
    /// <summary>
    /// Represents length-wise portion of a hold note.
    /// </summary>
    internal class BodyPiece : Container, IHasAccentColour
    {
        private readonly Box box;

        public BodyPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            Children = new[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.3f
                }
            };
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                if (accentColour == value)
                    return;
                accentColour = value;

                box.Colour = accentColour;
            }
        }
    }
}
