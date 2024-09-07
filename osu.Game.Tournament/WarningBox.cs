// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Tournament
{
    internal partial class WarningBox : Container
    {
        public WarningBox(string text)
        {
            Masking = true;
            CornerRadius = 10;
            Depth = float.MinValue;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Orange.Opacity(0.6f),
                    RelativeSizeAxes = Axes.Both,
                },
                new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                    Colour = Color4.White,
                    Size = new osuTK.Vector2(30),
                    Margin = new MarginPadding { Left = 10, Right = 10 },
                },
                new TournamentSpriteText
                {
                    Text = text,
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold),
                    Colour = Color4.White,
                    Padding = new MarginPadding { Left = 50, Top = 20, Bottom = 20, Right = 30 },
                },
            };
        }
    }
}
