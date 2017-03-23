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
    /// <summary>
    /// A component of the playfield that captures input and displays input as a drum.
    /// </summary>
    internal class InputDrum : Container
    {
        public InputDrum()
        {
            Size = new Vector2(TaikoPlayfield.PlayfieldHeight);

            Children = new Drawable[]
            {
                new TaikoHalfDrum(false)
                {
                    Name = "Left Half",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    RimKey = Key.D,
                    CentreKey = Key.F
                },
                new TaikoHalfDrum(true)
                {
                    Name = "Right Half",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Position = new Vector2(-1f, 0),
                    RimKey = Key.K,
                    CentreKey = Key.J
                }
            };
        }

        /// <summary>
        /// A half-drum. Contains one centre and one rim hit.
        /// </summary>
        private class TaikoHalfDrum : Container
        {
            /// <summary>
            /// The key to be used for the rim of the half-drum.
            /// </summary>
            public Key RimKey;
            
            /// <summary>
            /// The key to be used for the centre of the half-drum.
            /// </summary>
            public Key CentreKey;

            private Sprite rim;
            private Sprite rimHit;
            private Sprite centre;
            private Sprite centreHit;

            public TaikoHalfDrum(bool flipped)
            {
                Masking = true;

                Children = new Drawable[]
                {
                    rim = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both
                    },
                    rimHit = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        BlendingMode = BlendingMode.Additive
                    },
                    centre = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.7f)
                    },
                    centreHit = new Sprite
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
                rim.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer");
                rimHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-outer-hit");
                centre.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner");
                centreHit.Texture = textures.Get(@"Play/Taiko/taiko-drum-inner-hit");

                rimHit.Colour = colours.Blue;
                centreHit.Colour = colours.Pink;
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (args.Repeat)
                    return false;

                if (args.Key == CentreKey)
                {
                    centreHit.FadeIn();
                    centreHit.FadeOut(500, EasingTypes.OutQuint);
                }

                if (args.Key == RimKey)
                {
                    rimHit.FadeIn();
                    rimHit.FadeOut(500, EasingTypes.OutQuint);
                }

                return false;
            }
        }
    }
}
