// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Utils;
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

        #region Sound Effect Playback Parameters

        // swoosh-up
        private const double sfx_swoosh_pre_delay = 443f;
        private const double sfx_swoosh_volume = 0.4f;

        // score ticks
        private const double sfx_score_tick_debounce_rate_start = 18f;
        private const double sfx_score_tick_debounce_rate_end = 300f;
        private const Easing sfx_score_tick_debounce_rate_easing = Easing.OutSine;
        private const double sfx_score_tick_volume_start = 0.6f;
        private const double sfx_score_tick_volume_end = 1.0f;
        private const Easing sfx_score_tick_volume_easing = Easing.OutSine;
        private const Easing sfx_score_tick_pitch_easing = Easing.OutSine;

        // badge dinks
        private const double sfx_badge_dink_volume = 1f;

        // impact
        private const double sfx_rank_impact_volume = 1.0f;

        // applause
        private const double sfx_applause_pre_delay = 545f;
        private const double sfx_applause_volume = 0.8f;

        #endregion

        private readonly ScoreInfo score;

        private SmoothCircularProgress accuracyCircle;
        private SmoothCircularProgress innerMask;
        private Container<RankBadge> badges;
        private RankText rankText;

        private DrawableSample scoreTickSound;
        private DrawableSample badgeTickSound;
        private DrawableSample badgeMaxSound;
        private DrawableSample swooshUpSound;
        private DrawableSample rankImpactSound;
        private DrawableSample rankApplauseSound;

        private Bindable<double> tickPlaybackRate = new Bindable<double>();
        private double lastTickPlaybackTime;
        private bool isTicking;

        private readonly bool sfxEnabled;

        public AccuracyCircle(ScoreInfo score, bool sfxEnabled = false)
        {
            this.score = score;
            this.sfxEnabled = sfxEnabled;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, ISkinSource skin)
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

            if (sfxEnabled)
            {
                tickPlaybackRate = new Bindable<double>(sfx_score_tick_debounce_rate_start);

                switch (score.Rank)
                {
                    case ScoreRank.D:
                        rankImpactSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultRank_D)) as DrawableSample;
                        rankApplauseSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultApplause_D)) as DrawableSample;
                        break;

                    case ScoreRank.C:
                        rankImpactSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultRank_C)) as DrawableSample;
                        rankApplauseSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultApplause_C)) as DrawableSample;
                        break;

                    case ScoreRank.B:
                        rankImpactSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultRank_B)) as DrawableSample;
                        rankApplauseSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultApplause_B)) as DrawableSample;
                        break;

                    case ScoreRank.A:
                        rankImpactSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultRank_A)) as DrawableSample;
                        rankApplauseSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultApplause_A)) as DrawableSample;
                        break;

                    case ScoreRank.S:
                    case ScoreRank.SH:
                        rankImpactSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultRank_S)) as DrawableSample;
                        rankApplauseSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultApplause_S)) as DrawableSample;
                        break;

                    case ScoreRank.X:
                    case ScoreRank.XH:
                        rankImpactSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultRank_SS)) as DrawableSample;
                        rankApplauseSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultApplause_SS)) as DrawableSample;
                        break;
                }

                AddRangeInternal(new Drawable[]
                {
                    rankImpactSound,
                    rankApplauseSound,
                    scoreTickSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultScoreTick)) as DrawableSample,
                    badgeTickSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultBadgeTick)) as DrawableSample,
                    badgeMaxSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultBadgeTickMax)) as DrawableSample,
                    swooshUpSound = skin.GetDrawableComponent(new GameplaySkinComponent<GameplaySkinSamples>(GameplaySkinSamples.ResultSwooshUp)) as DrawableSample
                });
            }
        }

        private ScoreRank getRank(ScoreRank rank)
        {
            foreach (var mod in score.Mods.OfType<IApplicableToScoreProcessor>())
                rank = mod.AdjustRank(rank, score.Accuracy);

            return rank;
        }

        protected override void Update()
        {
            base.Update();

            if (!isTicking) return;

            bool enoughTimePassedSinceLastPlayback = Clock.CurrentTime - lastTickPlaybackTime >= tickPlaybackRate.Value;

            if (!enoughTimePassedSinceLastPlayback) return;

            Schedule(() => scoreTickSound?.Play());
            lastTickPlaybackTime = Clock.CurrentTime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(0).Then().ScaleTo(1, APPEAR_DURATION, Easing.OutQuint);

            if (sfxEnabled)
            {
                this.Delay(sfx_swoosh_pre_delay).Schedule(() =>
                {
                    swooshUpSound.VolumeTo(sfx_swoosh_volume);
                    swooshUpSound.Play();
                });
            }

            using (BeginDelayedSequence(RANK_CIRCLE_TRANSFORM_DELAY))
                innerMask.FillTo(1f, RANK_CIRCLE_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

            using (BeginDelayedSequence(ACCURACY_TRANSFORM_DELAY))
            {
                double targetAccuracy = score.Rank == ScoreRank.X || score.Rank == ScoreRank.XH ? 1 : Math.Min(1 - virtual_ss_percentage, score.Accuracy);

                accuracyCircle.FillTo(targetAccuracy, ACCURACY_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

                if (sfxEnabled)
                {
                    Schedule(() =>
                    {
                        scoreTickSound.FrequencyTo(1 + targetAccuracy, ACCURACY_TRANSFORM_DURATION, sfx_score_tick_pitch_easing);
                        scoreTickSound.VolumeTo(sfx_score_tick_volume_start).Then().VolumeTo(sfx_score_tick_volume_end, ACCURACY_TRANSFORM_DURATION, sfx_score_tick_volume_easing);
                        this.TransformBindableTo(tickPlaybackRate, sfx_score_tick_debounce_rate_end, ACCURACY_TRANSFORM_DURATION, sfx_score_tick_debounce_rate_easing);

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

                        if (sfxEnabled)
                        {
                            Schedule(() =>
                            {
                                DrawableSample dink = badgeNum < badges.Count - 1 ? badgeTickSound : badgeMaxSound;
                                dink.FrequencyTo(1 + badgeNum++ * 0.05);
                                dink.VolumeTo(sfx_badge_dink_volume);
                                dink.Play();
                            });
                        }
                    }
                }

                using (BeginDelayedSequence(TEXT_APPEAR_DELAY))
                {
                    rankText.Appear();

                    if (!sfxEnabled) return;

                    Schedule(() =>
                    {
                        isTicking = false;
                        rankImpactSound.VolumeTo(sfx_rank_impact_volume);
                        rankImpactSound.Play();
                    });

                    using (BeginDelayedSequence(sfx_applause_pre_delay))
                    {
                        Schedule(() =>
                        {
                            rankApplauseSound.VolumeTo(sfx_applause_volume);
                            rankApplauseSound.Play();
                        });
                    }
                }
            }
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
