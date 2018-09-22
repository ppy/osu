// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

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

            RelativeSizeAxes = Axes.Both;

            number = new OsuSpriteText
            {
                Text = @"1",
                Font = @"Venera",
                UseFullGlyphHeight = false,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = 40,
                Alpha = 1
            };
        }

        [BackgroundDependencyLoader]
        private void load(FontStore fontStore)
        {
            Texture numberTexture = fontStore.GetCharacter(number.Font, Text.ToString()[0]);
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
                new SkinnableDrawable($"Play/osu/default-{Text}", _ => new Sprite {
                    Texture = numberTexture,
                    Size =numberTexture.Size*number.TextSize/100,

                })
            };
        }
    }
}
