// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Intro
{
    public partial class StarRatingSequence : CompositeDrawable
    {
        private Container<Bar> bars = null!;
        private Container<StarRatingDisplay> starContainer = null!;
        private Container centerContainer = null!;
        private OsuSpriteText title = null!;
        private OsuSpriteText explainer = null!;

        private Sample? tickSample;
        private Sample? tickFinalSample;
        private Sample? ratingFoundSample;
        private Sample? noticeSample;

        private float lastTickStdDev;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, OverlayColourProvider overlayColourProvider, AudioManager audio)
        {
            Width = 600;
            AutoSizeAxes = Axes.Y;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0;

            InternalChild = new Container
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = overlayColourProvider.Background5,
                        Alpha = 0.8f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(30),
                        Spacing = new Vector2(10),
                        Direction = FillDirection.Vertical,
                        Children =
                        [
                            title = new OsuSpriteText
                            {
                                Text = "Refining star difficulty range...",
                                Padding = new MarginPadding { Bottom = 20 },
                                Font = OsuFont.Style.Title,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            centerContainer = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 90,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Masking = true,
                                CornerRadius = 8,
                                Children =
                                [
                                    new Box
                                    {
                                        Alpha = 0.4f,
                                        Colour = overlayColourProvider.Background4,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    bars = new Container<Bar>
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Top = 40, Horizontal = 3 },
                                    },
                                ]
                            },
                            starContainer = new Container<StarRatingDisplay>
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 20,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            explainer = new OsuSpriteText
                            {
                                Text = "Difficulty range is calculated to suit the two players.",
                                Padding = new MarginPadding { Top = 20 },
                                Font = OsuFont.Style.Heading2,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Alpha = 0,
                                AlwaysPresent = true,
                            }
                        ],
                    }
                }
            };

            for (int i = 0; i < 100; i++)
            {
                float difficulty = i / 10f;

                bars.Add(new Bar
                {
                    StarRating = difficulty,
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.X,
                    X = difficulty / 10f,
                    Width = 0.0075f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Colour = colour.ForStarDifficulty(Math.Max(difficulty, 0.1)),
                    Height = 0
                });

                if (i > 0 && i % 10 == 0)
                {
                    var starRatingDisplay = new StarRatingDisplay(new StarDifficulty(difficulty, 0), StarRatingDisplaySize.Small)
                    {
                        RelativePositionAxes = Axes.X,
                        X = difficulty / 10f,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(0)
                    };

                    starContainer.Add(starRatingDisplay);
                }
            }

            tickSample = audio.Samples.Get("Multiplayer/Matchmaking/Ranked/star-rating-tick");
            tickFinalSample = audio.Samples.Get("Multiplayer/Matchmaking/Ranked/star-rating-tick-final");
            ratingFoundSample = audio.Samples.Get("Multiplayer/Matchmaking/Ranked/star-rating-found");
            noticeSample = audio.Samples.Get("Multiplayer/Matchmaking/Ranked/star-rating-notice");
        }

        private float starRating { get; set; } = 5;

        private float amplitude { get; set; } = 0;

        private float stdDev { get; set; } = 6;

        private bool animateGaussianCurve;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public void Play(ref double delay, float starRating)
        {
            using (BeginDelayedSequence(delay))
                popIn();

            using (BeginDelayedSequence(delay += 500))
            {
                Schedule(() => animateGaussianCurve = true);

                this.TransformTo(nameof(starRating), starRating < 5 ? starRating + 4 : starRating - 4);
                this.TransformTo(nameof(starRating), starRating, 4000, new CubicBezierEasingFunction(easeIn: 0.3, easeOut: 0.5));
                this.TransformTo(nameof(amplitude), 1f, 4000, new CubicBezierEasingFunction(easeIn: 0.1, easeOut: 0.8));
                this.TransformTo(nameof(stdDev), 0.3f, 4500, new CubicBezierEasingFunction(easeIn: 0.2, easeOut: 0.7));

                explainer.Delay(400)
                         .FadeIn(200);
            }

            using (BeginDelayedSequence(delay += 5000))
            {
                Schedule(() =>
                {
                    animateGaussianCurve = false;

                    ratingFoundSample?.Play();

                    var container = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.BottomCentre,
                        AutoSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.X,
                        X = starRating * 0.1f,
                        Y = 34,
                        Colour = starRating < OsuColour.STAR_DIFFICULTY_DEFINED_COLOUR_CUTOFF ? colours.ForStarDifficulty(starRating) : colours.ForStarDifficultyText(starRating),
                        Spacing = new Vector2(4, 0),
                        Children =
                        [
                            new OsuSpriteText
                            {
                                Text = FormattableString.Invariant($"~{starRating:F2}"),
                                Font = OsuFont.GetFont(size: 24, weight: FontWeight.Bold),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            new SpriteIcon
                            {
                                Icon = FontAwesome.Solid.Star,
                                Size = new Vector2(19),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                        ]
                    };

                    centerContainer.Add(container);
                    // Avoid text getting masked out by inner containers
                    AddInternal(container.CreateProxy());

                    container.FadeInFromZero(200)
                             .ScaleTo(0)
                             .ScaleTo(1, 400, Easing.OutElasticQuarter);

                    title.Text = "Star rating has been decided!";

                    using (BeginDelayedSequence(1050))
                    {
                        explainer.FadeInFromZero(200);
                        Schedule(() =>
                        {
                            explainer.Text = "There's always a chance that you get maps outside this range";
                            noticeSample?.Play();
                        });
                    }
                });
            }
        }

        private void popIn()
        {
            this.FadeIn(200);

            foreach (var bar in bars)
            {
                double delay = Math.Abs(bar.StarRating - 5) * 50;

                bar.Delay(delay)
                   .ResizeHeightTo(0.1f, 300, Easing.OutExpo);
            }

            foreach (var drawable in starContainer)
            {
                double delay = Math.Abs((drawable.X * 10) - 5) * 50 + 100;

                drawable.Delay(delay)
                        .ScaleTo(0.8f, 400, Easing.OutElasticQuarter);
            }
        }

        public void PopOut()
        {
            foreach (var bar in bars)
            {
                double delay = Math.Abs(bar.StarRating - 5) * 50;

                bar.Delay(delay)
                   .ResizeHeightTo(0f, 300, Easing.OutExpo);
            }

            foreach (var drawable in starContainer)
            {
                double delay = Math.Abs((drawable.X * 10) - 5) * 50 + 100;

                drawable.Delay(delay)
                        .ScaleTo(0f, 400, Easing.OutElasticQuarter);
            }

            this.FadeOut(150);
        }

        protected override void Update()
        {
            base.Update();

            if (!animateGaussianCurve)
                return;

            const float min_alpha = 0.4f;

            foreach (var bar in bars)
            {
                float value = gaussianCurve(bar.StarRating, 1f, starRating, stdDev);

                bar.Height = float.Lerp(0.1f, 1f, value * amplitude);

                float targetAlpha = float.Clamp(min_alpha + value * 20f, min_alpha, 1);

                bar.Alpha = float.Lerp(targetAlpha, bar.Alpha, (float)Math.Exp(-0.01f * Time.Elapsed));
            }

            foreach (var child in starContainer)
            {
                float value = gaussianCurve(child.X * 10f, 1f, starRating, stdDev);

                float targetAlpha = float.Clamp(min_alpha + value * 20f, min_alpha, 1);

                child.Alpha = float.Lerp(targetAlpha, child.Alpha, (float)Math.Exp(-0.01f * Time.Elapsed));
            }

            static float gaussianCurve(float x, float amplitude, float center, float stdev)
            {
                float v1 = x - center;
                float v2 = (v1 * v1) / (2 * (stdev * stdev));
                return amplitude * MathF.Exp(-v2);
            }

            if (Math.Abs(lastTickStdDev - stdDev) <= 0.075) return;

            var tickChannel = tickSample!.GetChannel();
            tickChannel.Frequency.Value = 1 + amplitude * 0.3f;
            tickChannel.Volume.Value = 0.5 + amplitude * 0.5;
            tickChannel.Play();

            if (stdDev < 1)
            {
                var tickFinalChannel = tickFinalSample!.GetChannel();
                tickFinalChannel.Frequency.Value = 1 + amplitude * 0.3f;
                tickFinalChannel.Volume.Value = 0.1f + amplitude * 0.4f;
                tickFinalChannel.Play();
            }

            lastTickStdDev = stdDev;
        }

        private partial class Bar : CircularContainer
        {
            public required float StarRating;

            public Bar()
            {
                Masking = true;
                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                };
            }
        }
    }
}
