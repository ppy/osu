// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
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
                new Container
                {
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Radius = 60,
                        Colour = Color4.White.Opacity(0.5f),
                    },
                },
                number = new SkinnableSpriteText(new OsuSkinComponent(OsuSkinComponents.HitCircleText), _ => new OsuSpriteText
                {
                    Font = OsuFont.Numeric.With(size: 40),
                    UseFullGlyphHeight = false,
                }, confineMode: ConfineMode.NoScaling)
                {
                    Text = @"1"
                }
            };
        }
    }
}
