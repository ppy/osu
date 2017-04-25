// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class NumberPiece : Container
    {
        private readonly SpriteText number;

        public string Text
        {
            get { return number.Text; }
            set { number.Text = value; }
        }

        public NumberPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new CircularContainer
                {
                    Masking = true,
                    Origin = Anchor.Centre,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Glow,
                        Radius = 60,
                        Colour = Color4.White.Opacity(0.5f),
                    },
                    Children = new[]
                    {
                        new Box()
                    }
                },
                number = new OsuSpriteText
                {
                    Text = @"1",
                    Font = @"Venera",
                    UseFullGlyphHeight = false,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 40,
                    Alpha = 1
                }
            };
        }
    }
}