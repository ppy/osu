// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Intro
{
    public partial class VsSequence(UserWithRating player, UserWithRating opponent) : CompositeDrawable
    {
        private Drawable playerBackground = null!;
        private Drawable opponentBackground = null!;
        private Box flash = null!;
        private Drawable playerDisplay = null!;
        private Drawable opponentDisplay = null!;
        private CoverReveal opponentCoverReveal = null!;
        private CoverReveal playerCoverReveal = null!;
        private VsText vsText = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren =
            [
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = -100 },
                    Children =
                    [
                        playerBackground = new DelayedLoadWrapper(() => new PlayerCover(player.User)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }, timeBeforeLoad: 0)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.5f), Color4.White.Opacity(0.85f)),
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                        opponentBackground = new DelayedLoadWrapper(() => new PlayerCover(opponent.User)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }, timeBeforeLoad: 0)
                        {
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.5f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.85f), Color4.White.Opacity(0.5f)),
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                    ],
                },
                playerDisplay = new UserDisplay(player, Anchor.BottomLeft)
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding(70),
                    Alpha = 0,
                    AlwaysPresent = true,
                },
                opponentDisplay = new UserDisplay(opponent, Anchor.BottomRight)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.BottomLeft,
                    Margin = new MarginPadding(70),
                    Alpha = 0,
                    AlwaysPresent = true,
                },
                opponentCoverReveal = new CoverReveal(RankedPlayColourScheme.Red)
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Scale = new Vector2(-1, 1),
                    Alpha = 0,
                },
                playerCoverReveal = new CoverReveal(RankedPlayColourScheme.Blue)
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Alpha = 0,
                },
                flash = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
                vsText = new VsText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            ];
        }

        public void Play(ref double delay, out double impactDelay)
        {
            using (BeginDelayedSequence(delay))
            {
                this.FadeInFromZero(500);

                vsText.AnimateEntry(1000, Easing.OutExpo);
                vsText.ScaleTo(0.4f, 1300, Easing.OutExpo);
            }

            delay += 850;

            impactDelay = delay;

            using (BeginDelayedSequence(delay))
            {
                flash.FadeOutFromOne(500, Easing.Out);

                vsText.RevealText();

                playerCoverReveal.FadeIn();
                opponentCoverReveal.FadeIn();

                playerCoverReveal.Play();
                opponentCoverReveal.Play();

                playerBackground
                    .FadeIn()
                    .MoveToX(-40)
                    .MoveToX(40, 3000, new CubicBezierEasingFunction(0, 0.3, 0, 0.65))
                    .Then()
                    .MoveToX(100, 500, new CubicBezierEasingFunction(0.8, 0.05, 0.8, 0.8));

                opponentBackground
                    .FadeIn()
                    .MoveToX(40)
                    .MoveToX(-40, 3000, new CubicBezierEasingFunction(0, 0.3, 0, 0.65))
                    .Then()
                    .MoveToX(-100, 500, new CubicBezierEasingFunction(0.8, 0.05, 0.8, 0.8));

                playerDisplay
                    .FadeIn()
                    .MoveToX(-400)
                    .MoveToX(-100, 3000, new CubicBezierEasingFunction(0, 0.3, 0, 0.75))
                    .Then()
                    .MoveToX(800, 500, new CubicBezierEasingFunction(0.8, 0.05, 0.8, 0.8));

                opponentDisplay
                    .FadeIn()
                    .MoveToX(400)
                    .MoveToX(100, 3000, new CubicBezierEasingFunction(0, 0.6, 0, 0.75))
                    .Then()
                    .MoveToX(-800, 500, new CubicBezierEasingFunction(0.8, 0.05, 0.8, 0.8));

                vsText.Delay(3200)
                      .ScaleTo(0.25f, 400, Easing.InCubic);

                this.Delay(3200).FadeOut(300).Expire();
            }

            delay += 3350;
        }

        private partial class UserDisplay : CompositeDrawable
        {
            public UserDisplay(UserWithRating user, Anchor contentAnchor)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10),
                    Children =
                    [
                        new CircularContainer
                        {
                            Size = new Vector2(96),
                            Masking = true,
                            Anchor = contentAnchor,
                            Origin = contentAnchor,
                            Child = new DelayedLoadWrapper(() => new DrawableAvatar(user.User)
                            {
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fill,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }, timeBeforeLoad: 0)
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Anchor = contentAnchor,
                            Origin = contentAnchor,
                            Padding = new MarginPadding { Vertical = 10 },
                            Children =
                            [
                                new OsuSpriteText
                                {
                                    Text = FormattableString.Invariant($"Rating: {user.Rating:N0}"),
                                    Alpha = 0.8f,
                                    Font = OsuFont.Style.Title.With(size: 26),
                                    Anchor = contentAnchor,
                                    Origin = contentAnchor,
                                },
                                new OsuSpriteText
                                {
                                    Text = user.User.Username,
                                    Font = OsuFont.Style.Title.With(size: 40, weight: FontWeight.SemiBold),
                                    Anchor = contentAnchor,
                                    Origin = contentAnchor,
                                },
                            ]
                        }
                    ]
                };
            }
        }

        [LongRunningLoad]
        public partial class PlayerCover : CompositeDrawable
        {
            private readonly APIUser user;

            public PlayerCover(APIUser user)
            {
                this.user = user;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                Masking = true;

                AddInternal(new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get(user.CoverUrl),
                    FillMode = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                this.FadeInFromZero(250);
            }
        }

        private partial class VsText : CompositeDrawable
        {
            private Sprite vsText = null!;
            private LogoAnimation logoAnimation = null!;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AutoSizeAxes = Axes.Both;

                InternalChildren =
                [
                    vsText = new Sprite
                    {
                        Texture = textures.Get("Online/RankedPlay/vs"),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,
                    },
                    logoAnimation = new LogoAnimation
                    {
                        Texture = textures.Get("Online/RankedPlay/vs-animation"),
                    },
                ];
            }

            public void AnimateEntry(double duration, Easing easing)
            {
                logoAnimation.TransformTo(nameof(logoAnimation.AnimationProgress), 1f, duration, easing);
            }

            public void RevealText()
            {
                vsText.FadeIn();
                logoAnimation.FadeOut();
            }
        }
    }
}
