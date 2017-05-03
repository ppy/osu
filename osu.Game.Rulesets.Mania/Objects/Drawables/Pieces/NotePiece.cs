// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables.Pieces
{
    internal class NotePiece : Container, IHasAccentColour
    {
        private const float head_height = 10;
        private const float head_colour_height = 6;

        private Box colouredBox;

        public NotePiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = head_height;

            Masking = true;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                colouredBox = new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = head_colour_height,
                    Alpha = 0.5f
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

                colouredBox.Colour = AccentColour;

                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = AccentColour
                };
            }
        }
    }
}
