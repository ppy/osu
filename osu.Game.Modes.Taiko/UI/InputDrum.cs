using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.UI
{
    class InputDrum : Container
    {
        public InputDrum()
        {
            Size = new Vector2(86);

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

        class TaikoHalfDrum : Container
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
                        outer = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both
                        },
                        outerHit = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            Colour = new Color4(102, 204, 255, 255),
                            Alpha = 0,

                            BlendingMode = BlendingMode.Additive
                        },
                        inner = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f)
                        },
                        innerHit = new Sprite()
                        {
                            Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f),

                            Colour = new Color4(255, 102, 194, 255),
                            Alpha = 0,

                            BlendingMode = BlendingMode.Additive
                        }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                outer.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer");
                outerHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer-hit");
                inner.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner");
                innerHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner-hit");
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (args.Repeat)
                    return false;

                if (args.Key == Keys[0])
                {
                    innerHit.FadeIn();
                    innerHit.Delay(20).FadeOut(20);
                }

                if (args.Key == Keys[1])
                {
                    outerHit.FadeIn();
                    outerHit.Delay(20).FadeOut(20);
                }

                return false;
            }
        }
    }
}
