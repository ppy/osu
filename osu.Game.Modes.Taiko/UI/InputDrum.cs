// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.UI
{
    /// <summary>
    /// A component of the playfield that captures input and displays input as a drum.
    /// </summary>
    internal class InputDrum : Container
    {
        public InputDrum()
        {
            Size = new Vector2(TaikoPlayfield.PLAYFIELD_HEIGHT);

            const float middle_split = 10;

            Children = new Drawable[]
            {
                new TaikoHalfDrum(false)
                {
                    Name = "Left Half",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    X = -middle_split / 2,
                    RimKey = Key.D,
                    CentreKey = Key.F
                },
                new TaikoHalfDrum(true)
                {
                    Name = "Right Half",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    X = middle_split / 2,
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

            private readonly Sprite rim;
            private readonly Sprite rimHit;
            private readonly Sprite centre;
            private readonly Sprite centreHit;

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
                        BlendingMode = BlendingMode.Additive,
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

                Drawable target = null;

                if (args.Key == CentreKey)
                    target = centreHit;
                else if (args.Key == RimKey)
                    target = rimHit;

                if (target != null)
                {
                    target.FadeTo(Math.Min(target.Alpha + 0.4f, 1), 40, EasingTypes.OutQuint);
                    target.Delay(40);
                    target.FadeOut(1000, EasingTypes.OutQuint);
                }

                return false;
            }
        }
    }
}
