// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentSpriteTextWithBackground : CompositeDrawable
    {
        public readonly TournamentSpriteText Text;

        protected readonly Box Background;

        public TournamentSpriteTextWithBackground(string text = "", int fontSize = 50,
            Color4? backgroundColor = null, Color4? textColor = null, FontWeight? textWeight = FontWeight.SemiBold)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    Colour = backgroundColor.IsNotNull() ? backgroundColor.Value : TournamentGame.ELEMENT_BACKGROUND_COLOUR,
                    RelativeSizeAxes = Axes.Both,
                },
                Text = new TournamentSpriteText
                {
                    Colour = textColor.IsNotNull() ? textColor.Value : TournamentGame.ELEMENT_FOREGROUND_COLOUR,
                    Font = OsuFont.Torus.With(weight: textWeight, size: fontSize),
                    Padding = new MarginPadding { Left = 10, Right = 20 },
                    Text = text,
                }
            };
        }
    }
}
