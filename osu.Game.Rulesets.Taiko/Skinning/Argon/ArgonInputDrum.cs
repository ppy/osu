// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Screens.Ranking;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonInputDrum : AspectContainer
    {
        public static readonly ColourInfo RIM_HIT_GRADIENT = ColourInfo.GradientHorizontal(
            new Color4(227, 248, 255, 255),
            new Color4(198, 245, 255, 255)
        );

        public static readonly Colour4 RIM_HIT_GLOW = new Color4(126, 215, 253, 255);

        public static readonly ColourInfo CENTRE_HIT_GRADIENT = ColourInfo.GradientHorizontal(
            new Color4(255, 227, 236, 255),
            new Color4(255, 198, 211, 255)
        );

        public static readonly Colour4 CENTRE_HIT_GLOW = new Color4(255, 147, 199, 255);

        private const float rim_size = 0.3f;

        public ArgonInputDrum()
        {
            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float middle_split = 6;

            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(0.9f),
                Children = new Drawable[]
                {
                    new TaikoHalfDrum(false)
                    {
                        Name = "Left Half",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        RimAction = TaikoAction.LeftRim,
                        CentreAction = TaikoAction.LeftCentre
                    },
                    new TaikoHalfDrum(true)
                    {
                        Name = "Right Half",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        RimAction = TaikoAction.RightRim,
                        CentreAction = TaikoAction.RightCentre
                    },
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = OsuColour.Gray(38 / 255f),
                                Width = middle_split,
                                RelativeSizeAxes = Axes.Y,
                            },
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = OsuColour.Gray(48 / 255f),
                                Width = middle_split,
                                Height = 1 - rim_size,
                                RelativeSizeAxes = Axes.Y,
                            },
                        },
                    }
                }
            };
        }

        /// <summary>
        /// A half-drum. Contains one centre and one rim hit.
        /// </summary>
        private partial class TaikoHalfDrum : CompositeDrawable, IKeyBindingHandler<TaikoAction>
        {
            /// <summary>
            /// The key to be used for the rim of the half-drum.
            /// </summary>
            public TaikoAction RimAction;

            /// <summary>
            /// The key to be used for the centre of the half-drum.
            /// </summary>
            public TaikoAction CentreAction;

            private readonly Drawable rimHit;
            private readonly Drawable centreHit;

            public TaikoHalfDrum(bool flipped)
            {
                Anchor anchor = flipped ? Anchor.CentreLeft : Anchor.CentreRight;

                Masking = true;

                Anchor = anchor;
                Origin = anchor;

                RelativeSizeAxes = Axes.Both;
                // Extend maskable region for glow.
                Height = 2f;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.5f,
                        Children = new[]
                        {
                            new Circle
                            {
                                Anchor = anchor,
                                Colour = OsuColour.Gray(51 / 255f),
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both
                            },
                            rimHit = new Circle
                            {
                                Anchor = anchor,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = RIM_HIT_GRADIENT,
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Type = EdgeEffectType.Glow,
                                    Colour = RIM_HIT_GLOW.Opacity(0.66f),
                                    Radius = 50,
                                },
                                Alpha = 0,
                            },
                            new Circle
                            {
                                Anchor = anchor,
                                Origin = Anchor.Centre,
                                Colour = OsuColour.Gray(64 / 255f),
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(1 - rim_size)
                            },
                            centreHit = new Circle
                            {
                                Anchor = anchor,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = CENTRE_HIT_GRADIENT,
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Type = EdgeEffectType.Glow,
                                    Colour = CENTRE_HIT_GLOW,
                                    Radius = 50,
                                },
                                Size = new Vector2(1 - rim_size),
                                Alpha = 0,
                            }
                        },
                    },
                };
            }

            public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
            {
                Drawable? target = null;

                if (e.Action == CentreAction)
                    target = centreHit;
                else if (e.Action == RimAction)
                    target = rimHit;

                if (target != null)
                {
                    const float alpha_amount = 0.5f;

                    const float down_time = 40;
                    const float up_time = 750;

                    target.Animate(
                        t => t.FadeTo(Math.Min(target.Alpha + alpha_amount, 1), down_time, Easing.OutQuint)
                    ).Then(
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
