// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    /// <summary>
    /// The component that displays the player's accuracy on the results screen.
    /// </summary>
    public class AccuracyCircle : CompositeDrawable
    {
        /// <summary>
        /// Duration for the transforms causing this component to appear.
        /// </summary>
        public const double APPEAR_DURATION = 200;

        /// <summary>
        /// Delay before the accuracy circle starts filling.
        /// </summary>
        public const double ACCURACY_TRANSFORM_DELAY = 450;

        /// <summary>
        /// Duration for the accuracy circle fill.
        /// </summary>
        public const double ACCURACY_TRANSFORM_DURATION = 3000;

        /// <summary>
        /// Delay after <see cref="ACCURACY_TRANSFORM_DURATION"/> for the rank text (A/B/C/D/S/SS) to appear.
        /// </summary>
        public const double TEXT_APPEAR_DELAY = ACCURACY_TRANSFORM_DURATION / 2;

        /// <summary>
        /// Delay before the rank circles start filling.
        /// </summary>
        public const double RANK_CIRCLE_TRANSFORM_DELAY = 150;

        /// <summary>
        /// Duration for the rank circle fills.
        /// </summary>
        public const double RANK_CIRCLE_TRANSFORM_DURATION = 800;

        /// <summary>
        /// Relative width of the rank circles.
        /// </summary>
        public const float RANK_CIRCLE_RADIUS = 0.06f;

        /// <summary>
        /// Relative width of the circle showing the accuracy.
        /// </summary>
        private const float accuracy_circle_radius = 0.2f;

        /// <summary>
        /// SS is displayed as a 1% region, otherwise it would be invisible.
        /// </summary>
        private const double virtual_ss_percentage = 0.01;

        /// <summary>
        /// The easing for the circle filling transforms.
        /// </summary>
        public static readonly Easing ACCURACY_TRANSFORM_EASING = Easing.OutPow10;

        private readonly ScoreInfo score;

        private SmoothCircularProgress accuracyCircle;
        private SmoothCircularProgress innerMask;
        private Container<RankBadge> badges;
        private RankText rankText;

        private PoolableSkinnableSample scoreTickSound;
        private PoolableSkinnableSample badgeTickSound;
        private PoolableSkinnableSample badgeMaxSound;
        private PoolableSkinnableSample swooshUpSound;
        private PoolableSkinnableSample rankImpactSound;
        private PoolableSkinnableSample rankApplauseSound;

        private readonly Bindable<double> tickPlaybackRate = new Bindable<double>();

        private double lastTickPlaybackTime;
        private bool isTicking;

        private readonly bool withFlair;

        public AccuracyCircle(ScoreInfo score, bool withFlair = false)
        {
            this.score = score;
            this.withFlair = withFlair;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new SmoothCircularProgress
                {
                    Name = "Background circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(47),
                    Alpha = 0.5f,
                    InnerRadius = accuracy_circle_radius + 0.01f, // Extends a little bit into the circle
                    Current = { Value = 1 },
                },
                accuracyCircle = new SmoothCircularProgress
                {
                    Name = "Accuracy circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#7CF6FF"), Color4Extensions.FromHex("#BAFFA9")),
                    InnerRadius = accuracy_circle_radius,
                },
                new BufferedContainer
                {
                    Name = "Graded circles",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.8f),
                    Padding = new MarginPadding(2),
                    Children = new Drawable[]
                    {
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.X),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 1 }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.S),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 1 - virtual_ss_percentage }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.A),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.95f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.B),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.9f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.C),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.8f }
                        },
                        new SmoothCircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.ForRank(ScoreRank.D),
                            InnerRadius = RANK_CIRCLE_RADIUS,
                            Current = { Value = 0.7f }
                        },
                        new RankNotch(0),
                        new RankNotch((float)(1 - virtual_ss_percentage)),
                        new RankNotch(0.95f),
                        new RankNotch(0.9f),
                        new RankNotch(0.8f),
                        new RankNotch(0.7f),
                        new BufferedContainer
                        {
                            Name = "Graded circle mask",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(1),
                            Blending = new BlendingParameters
                            {
                                Source = BlendingType.DstColor,
                                Destination = BlendingType.OneMinusSrcAlpha,
                                SourceAlpha = BlendingType.One,
                                DestinationAlpha = BlendingType.SrcAlpha
                            },
                            Child = innerMask = new SmoothCircularProgress
                            {
                                RelativeSizeAxes = Axes.Both,
                                InnerRadius = RANK_CIRCLE_RADIUS - 0.01f,
                            }
                        }
                    }
                },
                badges = new Container<RankBadge>
                {
                    Name = "Rank badges",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Vertical = -15, Horizontal = -20 },
                    Children = new[]
                    {
                        new RankBadge(1f, getRank(ScoreRank.X)),
                        new RankBadge(0.95f, getRank(ScoreRank.S)),
                        new RankBadge(0.9f, getRank(ScoreRank.A)),
                        new RankBadge(0.8f, getRank(ScoreRank.B)),
                        new RankBadge(0.7f, getRank(ScoreRank.C)),
                        new RankBadge(0.35f, getRank(ScoreRank.D)),
                    }
                },
                rankText = new RankText(score.Rank)
            };

            if (withFlair)
            {
                AddRangeInternal(new Drawable[]
                {
                    rankImpactSound = new PoolableSkinnableSample(new SampleInfo(impactSampleName)),
                    rankApplauseSound = new PoolableSkinnableSample(new SampleInfo(@"applause", applauseSampleName)),
                    scoreTickSound = new PoolableSkinnableSample(new SampleInfo(@"Results/score-tick")),
                    badgeTickSound = new PoolableSkinnableSample(new SampleInfo(@"Results/badge-dink")),
                    badgeMaxSound = new PoolableSkinnableSample(new SampleInfo(@"Results/badge-dink-max")),
                    swooshUpSound = new PoolableSkinnableSample(new SampleInfo(@"Results/swoosh-up")),
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(0).Then().ScaleTo(1, APPEAR_DURATION, Easing.OutQuint);

            if (withFlair)
            {
                const double swoosh_pre_delay = 443f;
                const double swoosh_volume = 0.4f;

                this.Delay(swoosh_pre_delay).Schedule(() =>
                {
                    swooshUpSound.VolumeTo(swoosh_volume);
                    swooshUpSound.Play();
                });
            }

            using (BeginDelayedSequence(RANK_CIRCLE_TRANSFORM_DELAY))
                innerMask.FillTo(1f, RANK_CIRCLE_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

            using (BeginDelayedSequence(ACCURACY_TRANSFORM_DELAY))
            {
                double targetAccuracy = score.Rank == ScoreRank.X || score.Rank == ScoreRank.XH ? 1 : Math.Min(1 - virtual_ss_percentage, score.Accuracy);

                accuracyCircle.FillTo(targetAccuracy, ACCURACY_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

                if (withFlair)
                {
                    Schedule(() =>
                    {
                        const double score_tick_debounce_rate_start = 18f;
                        const double score_tick_debounce_rate_end = 300f;
                        const double score_tick_volume_start = 0.6f;
                        const double score_tick_volume_end = 1.0f;

                        this.TransformBindableTo(tickPlaybackRate, score_tick_debounce_rate_start);
                        this.TransformBindableTo(tickPlaybackRate, score_tick_debounce_rate_end, ACCURACY_TRANSFORM_DURATION, Easing.OutSine);

                        scoreTickSound.FrequencyTo(1 + targetAccuracy, ACCURACY_TRANSFORM_DURATION, Easing.OutSine);
                        scoreTickSound.VolumeTo(score_tick_volume_start).Then().VolumeTo(score_tick_volume_end, ACCURACY_TRANSFORM_DURATION, Easing.OutSine);

                        isTicking = true;
                    });
                }

                int badgeNum = 0;

                foreach (var badge in badges)
                {
                    if (badge.Accuracy > score.Accuracy)
                        continue;

                    using (BeginDelayedSequence(inverseEasing(ACCURACY_TRANSFORM_EASING, Math.Min(1 - virtual_ss_percentage, badge.Accuracy) / targetAccuracy) * ACCURACY_TRANSFORM_DURATION))
                    {
                        badge.Appear();

                        if (withFlair)
                        {
                            Schedule(() =>
                            {
                                var dink = badgeNum < badges.Count - 1 ? badgeTickSound : badgeMaxSound;

                                dink.FrequencyTo(1 + badgeNum++ * 0.05);
                                dink.Play();
                            });
                        }
                    }
                }

                using (BeginDelayedSequence(TEXT_APPEAR_DELAY))
                {
                    rankText.Appear();

                    if (!withFlair) return;

                    Schedule(() =>
                    {
                        isTicking = false;
                        rankImpactSound.Play();
                    });

                    const double applause_pre_delay = 545f;
                    const double applause_volume = 0.8f;

                    using (BeginDelayedSequence(applause_pre_delay))
                    {
                        Schedule(() =>
                        {
                            rankApplauseSound.VolumeTo(applause_volume);
                            rankApplauseSound.Play();
                        });
                    }
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isTicking && Clock.CurrentTime - lastTickPlaybackTime >= tickPlaybackRate.Value)
            {
                scoreTickSound?.Play();
                lastTickPlaybackTime = Clock.CurrentTime;
            }
        }

        private string applauseSampleName
        {
            get
            {
                switch (score.Rank)
                {
                    default:
                    case ScoreRank.D:
                        return @"Results/applause-d";

                    case ScoreRank.C:
                        return @"Results/applause-c";

                    case ScoreRank.B:
                        return @"Results/applause-b";

                    case ScoreRank.A:
                        return @"Results/applause-a";

                    case ScoreRank.S:
                    case ScoreRank.SH:
                    case ScoreRank.X:
                    case ScoreRank.XH:
                        return @"Results/applause-s";
                }
            }
        }

        private string impactSampleName
        {
            get
            {
                switch (score.Rank)
                {
                    default:
                    case ScoreRank.D:
                        return @"Results/rank-impact-fail-d";

                    case ScoreRank.C:
                    case ScoreRank.B:
                        return @"Results/rank-impact-fail";

                    case ScoreRank.A:
                    case ScoreRank.S:
                    case ScoreRank.SH:
                        return @"Results/rank-impact-pass";

                    case ScoreRank.X:
                    case ScoreRank.XH:
                        return @"Results/rank-impact-pass-ss";
                }
            }
        }

        private ScoreRank getRank(ScoreRank rank)
        {
            foreach (var mod in score.Mods.OfType<IApplicableToScoreProcessor>())
                rank = mod.AdjustRank(rank, score.Accuracy);

            return rank;
        }

        private double inverseEasing(Easing easing, double targetValue)
        {
            double test = 0;
            double result = 0;
            int count = 2;

            while (Math.Abs(result - targetValue) > 0.005)
            {
                int dir = Math.Sign(targetValue - result);

                test += dir * 1.0 / count;
                result = Interpolation.ApplyEasing(easing, test);

                count++;
            }

            return test;
        }
    }
}
