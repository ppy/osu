// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// A component of the playfield that captures input and displays input as a drum.
    /// </summary>
    internal class InputDrum : Container
    {
        private const float middle_split = 0.025f;

        [Cached]
        private DrumSampleTriggerSource sampleTriggerSource;

        public InputDrum(HitObjectContainer hitObjectContainer)
        {
            sampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer);

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.InputDrum), _ => new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Scale = new Vector2(0.9f),
                    Children = new Drawable[]
                    {
                        new TaikoHalfDrum(false)
                        {
                            Name = "Left Half",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.X,
                            X = -middle_split / 2,
                            RimAction = TaikoAction.LeftRim,
                            CentreAction = TaikoAction.LeftCentre
                        },
                        new TaikoHalfDrum(true)
                        {
                            Name = "Right Half",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.X,
                            X = middle_split / 2,
                            RimAction = TaikoAction.RightRim,
                            CentreAction = TaikoAction.RightCentre
                        }
                    }
                }),
                sampleTriggerSource
            };
        }

        /// <summary>
        /// A half-drum. Contains one centre and one rim hit.
        /// </summary>
        private class TaikoHalfDrum : Container, IKeyBindingHandler<TaikoAction>
        {
            /// <summary>
            /// The key to be used for the rim of the half-drum.
            /// </summary>
            public TaikoAction RimAction;

            /// <summary>
            /// The key to be used for the centre of the half-drum.
            /// </summary>
            public TaikoAction CentreAction;

            private readonly Sprite rim;
            private readonly Sprite rimHit;
            private readonly Sprite centre;
            private readonly Sprite centreHit;

            [Resolved]
            private DrumSampleTriggerSource sampleTriggerSource { get; set; }

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
                        Blending = BlendingParameters.Additive,
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
                        Blending = BlendingParameters.Additive
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures, OsuColour colours)
            {
                rim.Texture = textures.Get(@"Gameplay/taiko/taiko-drum-outer");
                rimHit.Texture = textures.Get(@"Gameplay/taiko/taiko-drum-outer-hit");
                centre.Texture = textures.Get(@"Gameplay/taiko/taiko-drum-inner");
                centreHit.Texture = textures.Get(@"Gameplay/taiko/taiko-drum-inner-hit");

                rimHit.Colour = colours.Blue;
                centreHit.Colour = colours.Pink;
            }

            public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
            {
                Drawable target = null;
                Drawable back = null;

                if (e.Action == CentreAction)
                {
                    target = centreHit;
                    back = centre;

                    sampleTriggerSource.Play(HitType.Centre);
                }
                else if (e.Action == RimAction)
                {
                    target = rimHit;
                    back = rim;

                    sampleTriggerSource.Play(HitType.Rim);
                }

                if (target != null)
                {
                    const float scale_amount = 0.05f;
                    const float alpha_amount = 0.5f;

                    const float down_time = 40;
                    const float up_time = 1000;

                    back.ScaleTo(target.Scale.X - scale_amount, down_time, Easing.OutQuint)
                        .Then()
                        .ScaleTo(1, up_time, Easing.OutQuint);

                    target.Animate(
                        t => t.ScaleTo(target.Scale.X - scale_amount, down_time, Easing.OutQuint),
                        t => t.FadeTo(Math.Min(target.Alpha + alpha_amount, 1), down_time, Easing.OutQuint)
                    ).Then(
                        t => t.ScaleTo(1, up_time, Easing.OutQuint),
                        t => t.FadeOut(up_time, Easing.OutQuint)
                    );
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
            {
            }
        }
    }
}
