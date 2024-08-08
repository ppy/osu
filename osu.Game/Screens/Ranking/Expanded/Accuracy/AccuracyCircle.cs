// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    /// <summary>
    /// The component that displays the player's accuracy on the results screen.
    /// </summary>
    public partial class AccuracyCircle : CompositeDrawable
    {
        /// <summary>
        /// The total duration of the animation.
        /// </summary>
        public const double TOTAL_DURATION = APPEAR_DURATION + ACCURACY_TRANSFORM_DELAY + ACCURACY_TRANSFORM_DURATION;

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
        public const float RANK_CIRCLE_RADIUS = 0.05f;

        /// <summary>
        /// Relative width of the circle showing the accuracy.
        /// </summary>
        private const float accuracy_circle_radius = 0.2f;

        /// <summary>
        /// SS is displayed as a 1% region, otherwise it would be invisible.
        /// </summary>
        public const double VIRTUAL_SS_PERCENTAGE = 0.01;

        /// <summary>
        /// The width of spacing in terms of accuracy between the grade circles.
        /// </summary>
        public const double GRADE_SPACING_PERCENTAGE = 2.0 / 360;

        /// <summary>
        /// The easing for the circle filling transforms.
        /// </summary>
        public static readonly Easing ACCURACY_TRANSFORM_EASING = Easing.OutPow10;

        private readonly ScoreInfo score;

        private CircularProgress accuracyCircle = null!;
        private GradedCircles gradedCircles = null!;
        private Container<RankBadge> badges = null!;
        private RankText rankText = null!;

        private PoolableSkinnableSample? scoreTickSound;
        private PoolableSkinnableSample? badgeTickSound;
        private PoolableSkinnableSample? badgeMaxSound;
        private PoolableSkinnableSample? swooshUpSound;
        private PoolableSkinnableSample? rankImpactSound;
        private PoolableSkinnableSample? rankApplauseSound;

        private readonly Bindable<double> tickPlaybackRate = new Bindable<double>();

        private double lastTickPlaybackTime;
        private bool isTicking;

        private readonly double accuracyX;
        private readonly double accuracyS;
        private readonly double accuracyA;
        private readonly double accuracyB;
        private readonly double accuracyC;
        private readonly double accuracyD;
        private readonly bool withFlair;

        private readonly bool isFailedSDueToMisses;
        private RankText failedSRankText = null!;

        public AccuracyCircle(ScoreInfo score, bool withFlair = false)
        {
            this.score = score;
            this.withFlair = withFlair;

            ScoreProcessor scoreProcessor = score.Ruleset.CreateInstance().CreateScoreProcessor();
            accuracyX = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.X);
            accuracyS = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.S);

            accuracyA = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.A);
            accuracyB = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.B);
            accuracyC = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.C);
            accuracyD = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.D);

            isFailedSDueToMisses = score.Accuracy >= accuracyS && score.Rank == ScoreRank.A;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new CircularProgress
                {
                    Name = "Background circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(47),
                    Alpha = 0.5f,
                    InnerRadius = accuracy_circle_radius + 0.01f, // Extends a little bit into the circle
                    Progress = 1,
                },
                accuracyCircle = new CircularProgress
                {
                    Name = "Accuracy circle",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#7CF6FF"), Color4Extensions.FromHex("#BAFFA9")),
                    InnerRadius = accuracy_circle_radius,
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.8f),
                    Padding = new MarginPadding(2.5f),
                    Child = gradedCircles = new GradedCircles(accuracyC, accuracyB, accuracyA, accuracyS, accuracyX)
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                },
                badges = new Container<RankBadge>
                {
                    Name = "Rank badges",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Vertical = -15, Horizontal = -20 },
                    Children = new[]
                    {
                        new RankBadge(accuracyD, Interpolation.Lerp(accuracyD, accuracyC, 0.5), getRank(ScoreRank.D)),
                        new RankBadge(accuracyC, Interpolation.Lerp(accuracyC, accuracyB, 0.5), getRank(ScoreRank.C)),
                        new RankBadge(accuracyB, Interpolation.Lerp(accuracyB, accuracyA, 0.5), getRank(ScoreRank.B)),
                        // The S and A badges are moved down slightly to prevent collision with the SS badge.
                        new RankBadge(accuracyA, Interpolation.Lerp(accuracyA, accuracyS, 0.25), getRank(ScoreRank.A)),
                        new RankBadge(accuracyS, Interpolation.Lerp(accuracyS, (accuracyX - VIRTUAL_SS_PERCENTAGE), 0.25), getRank(ScoreRank.S)),
                        new RankBadge(accuracyX, accuracyX, getRank(ScoreRank.X)),
                    }
                },
                rankText = new RankText(score.Rank)
            };

            if (isFailedSDueToMisses)
                AddInternal(failedSRankText = new RankText(ScoreRank.S));

            if (withFlair)
            {
                var applauseSamples = new List<string> { applauseSampleName };
                if (score.Rank >= ScoreRank.B)
                    // when rank is B or higher, play legacy applause sample on legacy skins.
                    applauseSamples.Insert(0, @"applause");

                AddRangeInternal(new Drawable[]
                {
                    rankImpactSound = new PoolableSkinnableSample(new SampleInfo(impactSampleName)),
                    rankApplauseSound = new PoolableSkinnableSample(new SampleInfo(applauseSamples.ToArray())),
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
                    swooshUpSound!.VolumeTo(swoosh_volume);
                    swooshUpSound!.Play();
                });
            }

            using (BeginDelayedSequence(RANK_CIRCLE_TRANSFORM_DELAY))
                gradedCircles.TransformTo(nameof(GradedCircles.Progress), 1.0, RANK_CIRCLE_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

            using (BeginDelayedSequence(ACCURACY_TRANSFORM_DELAY))
            {
                double targetAccuracy = score.Accuracy;
                double[] notchPercentages =
                {
                    accuracyS,
                    accuracyA,
                    accuracyB,
                    accuracyC,
                };

                // Ensure the gauge overshoots or undershoots a bit so it doesn't land in the gaps of the inner graded circle (caused by `RankNotch`es),
                // to prevent ambiguity on what grade it's pointing at.
                foreach (double p in notchPercentages)
                {
                    if (Precision.AlmostEquals(p, targetAccuracy, GRADE_SPACING_PERCENTAGE / 2))
                    {
                        int tippingDirection = targetAccuracy - p >= 0 ? 1 : -1; // We "round up" here to match rank criteria
                        targetAccuracy = p + tippingDirection * (GRADE_SPACING_PERCENTAGE / 2);
                        break;
                    }
                }

                // The final gap between 99.999...% (S) and 100% (SS) is exaggerated by `virtual_ss_percentage`. We don't want to land there either.
                if (score.Rank == ScoreRank.X || score.Rank == ScoreRank.XH)
                    targetAccuracy = 1;
                else
                    targetAccuracy = Math.Min(accuracyX - VIRTUAL_SS_PERCENTAGE - GRADE_SPACING_PERCENTAGE / 2, targetAccuracy);

                // The accuracy circle gauge visually fills up a bit too much.
                // This wouldn't normally matter but we want it to align properly with the inner graded circle in the above cases.
                const double visual_alignment_offset = 0.001;

                if (targetAccuracy < 1 && targetAccuracy >= visual_alignment_offset)
                    targetAccuracy -= visual_alignment_offset;

                accuracyCircle.ProgressTo(targetAccuracy, ACCURACY_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

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

                        scoreTickSound!.FrequencyTo(1 + targetAccuracy, ACCURACY_TRANSFORM_DURATION, Easing.OutSine);
                        scoreTickSound!.VolumeTo(score_tick_volume_start).Then().VolumeTo(score_tick_volume_end, ACCURACY_TRANSFORM_DURATION, Easing.OutSine);

                        isTicking = true;
                    });
                }

                int badgeNum = 0;

                if (score.Rank != ScoreRank.F)
                {
                    foreach (var badge in badges)
                    {
                        if (badge.Accuracy > score.Accuracy)
                            continue;

                        using (BeginDelayedSequence(
                                   inverseEasing(ACCURACY_TRANSFORM_EASING, Math.Min(accuracyX - VIRTUAL_SS_PERCENTAGE, badge.Accuracy) / targetAccuracy) * ACCURACY_TRANSFORM_DURATION))
                        {
                            badge.Appear();

                            if (withFlair)
                            {
                                Schedule(() =>
                                {
                                    var dink = badgeNum < badges.Count - 1 ? badgeTickSound : badgeMaxSound;

                                    dink!.FrequencyTo(1 + badgeNum++ * 0.05);
                                    dink!.Play();
                                });
                            }
                        }
                    }
                }

                using (BeginDelayedSequence(TEXT_APPEAR_DELAY))
                {
                    rankText.Appear();

                    if (withFlair)
                    {
                        Schedule(() =>
                        {
                            isTicking = false;
                            rankImpactSound!.Play();
                        });

                        const double applause_pre_delay = 545f;
                        const double applause_volume = 0.8f;

                        using (BeginDelayedSequence(applause_pre_delay))
                        {
                            Schedule(() =>
                            {
                                rankApplauseSound!.VolumeTo(applause_volume);
                                rankApplauseSound!.Play();
                            });
                        }
                    }
                }

                if (isFailedSDueToMisses)
                {
                    const double adjust_duration = 200;

                    using (BeginDelayedSequence(TEXT_APPEAR_DELAY - adjust_duration))
                    {
                        failedSRankText.FadeIn(adjust_duration);

                        using (BeginDelayedSequence(adjust_duration))
                        {
                            failedSRankText
                                .FadeColour(Color4.Red, 800, Easing.Out)
                                .RotateTo(10, 1000, Easing.Out)
                                .MoveToY(100, 1000, Easing.In)
                                .FadeOut(800, Easing.Out);

                            accuracyCircle
                                .ProgressTo(accuracyS - GRADE_SPACING_PERCENTAGE / 2 - visual_alignment_offset, 70, Easing.OutQuint);

                            badges.Single(b => b.Rank == getRank(ScoreRank.S))
                                  .FadeOut(70, Easing.OutQuint);
                        }
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
