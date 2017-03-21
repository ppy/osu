// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Graphics;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko.UI
{
    internal class InputDrum : Container
    {
        public InputDrum()
        {
            Size = new Vector2(TaikoPlayfield.PlayfieldHeight);

            Children = new Drawable[]
            {
                new TaikoHalfDrum(false)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,

                    RelativeSizeAxes = Axes.Both,

                    Keys = new List<Key>(new[] { Key.F, Key.D })
                },
                new TaikoHalfDrum(true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,

                    RelativeSizeAxes = Axes.Both,

                    Position = new Vector2(-1f, 0),

                    Keys = new List<Key>(new[] { Key.J, Key.K })
                }
            };
        }

        private class TaikoHalfDrum : Container
        {
            /// <summary>
            /// Keys[0] -> Inner key
            /// Keys[0] -> Outer key
            /// </summary>
            public List<Key> Keys = new List<Key>();

            private Sprite outer;
            private Sprite outerHit;
            private Sprite inner;
            private Sprite innerHit;

            public TaikoHalfDrum(bool flipped)
            {
                Masking = true;

                Children = new Drawable[]
                {
                    outer = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both
                    },
                    outerHit = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,

                        Alpha = 0,

                        BlendingMode = BlendingMode.Additive
                    },
                    inner = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.7f)
                    },
                    innerHit = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = Anchor.Centre,

                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.7f),

                        Alpha = 0,

                        BlendingMode = BlendingMode.Additive
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures, OsuColour colours)
            {
                outer.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer");
                outerHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer-hit");
                inner.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner");
                innerHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner-hit");

                outerHit.Colour = colours.Blue;
                innerHit.Colour = colours.Pink;
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (args.Repeat)
                    return false;

                if (args.Key == Keys[0])
                {
                    innerHit.FadeIn();
                    innerHit.FadeOut(500, EasingTypes.OutQuint);
                }

                if (args.Key == Keys[1])
                {
                    outerHit.FadeIn();
                    outerHit.FadeOut(500, EasingTypes.OutQuint);
                }

                return false;
            }
        }
    }
}
