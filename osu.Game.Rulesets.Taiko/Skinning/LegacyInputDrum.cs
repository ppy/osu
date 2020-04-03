// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Taiko.Audio;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    /// <summary>
    /// A component of the playfield that captures input and displays input as a drum.
    /// </summary>
    internal class LegacyInputDrum : Container
    {
        public LegacyInputDrum()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Children = new Drawable[]
            {
                new Sprite
                {
                    Texture = skin.GetTexture("taiko-bar-left")
                },
                new LegacyHalfDrum(false)
                {
                    Name = "Left Half",
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    RimAction = TaikoAction.LeftRim,
                    CentreAction = TaikoAction.LeftCentre
                },
                new LegacyHalfDrum(true)
                {
                    Name = "Right Half",
                    Anchor = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Scale = new Vector2(-1, 1),
                    RimAction = TaikoAction.RightRim,
                    CentreAction = TaikoAction.RightCentre
                }
            };
        }

        /// <summary>
        /// A half-drum. Contains one centre and one rim hit.
        /// </summary>
        private class LegacyHalfDrum : Container, IKeyBindingHandler<TaikoAction>
        {
            /// <summary>
            /// The key to be used for the rim of the half-drum.
            /// </summary>
            public TaikoAction RimAction;

            /// <summary>
            /// The key to be used for the centre of the half-drum.
            /// </summary>
            public TaikoAction CentreAction;

            private readonly Sprite rimHit;
            private readonly Sprite centreHit;

            [Resolved]
            private DrumSampleMapping sampleMappings { get; set; }

            public LegacyHalfDrum(bool flipped)
            {
                Masking = true;

                Children = new Drawable[]
                {
                    rimHit = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = flipped ? Anchor.CentreLeft : Anchor.CentreRight,
                        Scale = new Vector2(-1, 1),
                        Alpha = 0,
                    },
                    centreHit = new Sprite
                    {
                        Anchor = flipped ? Anchor.CentreRight : Anchor.CentreLeft,
                        Origin = flipped ? Anchor.CentreRight : Anchor.CentreLeft,
                        Alpha = 0,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                rimHit.Texture = skin.GetTexture(@"taiko-drum-outer");
                centreHit.Texture = skin.GetTexture(@"taiko-drum-inner");
            }

            public bool OnPressed(TaikoAction action)
            {
                Drawable target = null;
                var drumSample = sampleMappings.SampleAt(Time.Current);

                if (action == CentreAction)
                {
                    target = centreHit;
                    drumSample.Centre?.Play();
                }
                else if (action == RimAction)
                {
                    target = rimHit;
                    drumSample.Rim?.Play();
                }

                if (target != null)
                {
                    const float alpha_amount = 1;

                    const float down_time = 80;
                    const float up_time = 50;

                    target.Animate(
                        t => t.FadeTo(Math.Min(target.Alpha + alpha_amount, 1), down_time)
                    ).Then(
                        t => t.FadeOut(up_time)
                    );
                }

                return false;
            }

            public void OnReleased(TaikoAction action)
            {
            }
        }
    }
}
