// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class NumberPiece : Container
    {
        private readonly SkinnableSpriteText number;

        public string Text
        {
            get => number.Text;
            set => number.Text = value;
        }

        public NumberPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new SkinnableDrawable("Play/osu/number-glow", name => new CircularContainer
                {
                    Masking = true,
                    Origin = Anchor.Centre,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Radius = 60,
                        Colour = Color4.White.Opacity(0.5f),
                    },
                    Child = new Box()
                }, s => s.GetTexture("Play/osu/hitcircle") == null),
                number = new SkinnableSpriteText("Play/osu/number-text", _ => new OsuSpriteText
                {
                    Font = OsuFont.Numeric.With(size: 40),
                    UseFullGlyphHeight = false,
                }, restrictSize: false)
                {
                    Text = @"1"
                }
            };
        }
    }
}
