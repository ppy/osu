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
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class ArgonScoreRankDisplay : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        /// <summary>
        /// SS is displayed as a 1% region, otherwise it would be invisible.
        /// </summary>
        public const double VIRTUAL_SS_PERCENTAGE = 0.01;

        /// <summary>
        /// Delay before the accuracy circle starts filling.
        /// </summary>
        public const double ACCURACY_TRANSFORM_DELAY = 450;

        private Container gradedCirclesContainer = null!;
        private GradedCirclesV2 gradedCircles = null!;
        private Sprite rankSprite = null!;
        private Sprite rankSpriteAdditive = null!;

        private Drawable glowLayer = null!;

        private Container<PoolableSkinnableSample> sampleContainer = null!;
        private PoolableSkinnableSample rankImpactFailSound = null!;
        private PoolableSkinnableSample rankImpactPassSound = null!;
        private PoolableSkinnableSample rankImpactPassSSSound = null!;
        private PoolableSkinnableSample scoreTickSound = null!;
        private PoolableSkinnableSample badgeTickSound = null!;
        private PoolableSkinnableSample badgeMaxSound = null!;
        private PoolableSkinnableSample swooshUpSound = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Size = new Vector2(640);
            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;
            Masking = true;

            InternalChildren =
            [
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 2,
                    Masking = true,
                    EdgeEffect = ArgonBeatmapInfoPanel.CreateShadowEdgeEffect(),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientHorizontal(
                            colourProvider.Background4.Opacity(0.99f),
                            colourProvider.Background4.Opacity(0.9f)
                        ),
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(20),
                    Children =
                    [
                        gradedCirclesContainer = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            EdgeEffect = ArgonBeatmapInfoPanel.CreateShadowEdgeEffect()
                        },
                        rankSprite = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fit,
                        },
                        rankSpriteAdditive = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fit,
                            Alpha = 0,
                            Blending = BlendingParameters.Additive,
                        },
                    ]
                },
                glowLayer = new Box
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Blending = BlendingParameters.Additive,
                },
                sampleContainer = new Container<PoolableSkinnableSample>
                {
                    Children =
                    [
                        rankImpactFailSound = new PoolableSkinnableSample(new SampleInfo(@"Results/rank-impact-fail")),
                        rankImpactPassSound = new PoolableSkinnableSample(new SampleInfo(@"Results/rank-impact-pass")),
                        rankImpactPassSSSound = new PoolableSkinnableSample(new SampleInfo(@"Results/rank-impact-pass-ss")),
                        scoreTickSound = new PoolableSkinnableSample(new SampleInfo(@"Results/score-tick")),
                        badgeTickSound = new PoolableSkinnableSample(new SampleInfo(@"Results/badge-dink")),
                        badgeMaxSound = new PoolableSkinnableSample(new SampleInfo(@"Results/badge-dink-max")),
                        swooshUpSound = new PoolableSkinnableSample(new SampleInfo(@"Results/swoosh-up")),
                    ],
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            var scoreProcessor = score.Value.Ruleset.CreateInstance().CreateScoreProcessor();

            gradedCirclesContainer.Child = gradedCircles = new GradedCirclesV2(scoreProcessor)
            {
                RelativeSizeAxes = Axes.Both,
                Progress = getTargetAccuracy(scoreProcessor, animated: false),
            };

            glowLayer.Colour = ColourInfo.GradientHorizontal(
                OsuColour.ForRank(score.Value.Rank).Opacity(0.0f),
                OsuColour.ForRank(score.Value.Rank).Opacity(0.3f)
            );

            rankSprite.Texture = rankSpriteAdditive.Texture = skinManager.DefaultClassicSkin.GetTexture(DrawableRank.GetLegacyRankTextureName(score.Value.Rank));
        }

        private double getTargetAccuracy(ScoreProcessor scoreProcessor, bool animated)
        {
            double accuracyS = scoreProcessor.AccuracyCutoffFromRank(ScoreRank.S);
            bool isFailedSDueToMisses = score.Value.Accuracy >= accuracyS && score.Value.Rank == ScoreRank.A;

            if (isFailedSDueToMisses && !animated)
                return accuracyS;

            return score.Value.Rank == ScoreRank.X || score.Value.Rank == ScoreRank.XH ? 1 : Math.Min(score.Value.Accuracy, 1 - VIRTUAL_SS_PERCENTAGE);
        }

        protected override void Update()
        {
            base.Update();

            if (isTicking && Clock.CurrentTime - lastTickPlaybackTime >= tickPlaybackRate.Value)
            {
                scoreTickSound.Play();
                lastTickPlaybackTime = Clock.CurrentTime;
            }
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the argon skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt(2)
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        private readonly Bindable<double> tickPlaybackRate = new Bindable<double>();
        private bool isTicking;
        private double lastTickPlaybackTime;

        public double StartAnimating(double startTime)
        {
            FinishAnimating();

            var scoreProcessor = score.Value.Ruleset.CreateInstance().CreateScoreProcessor();
            double initialAccuracy = getTargetAccuracy(scoreProcessor, animated: true);
            double finalAccuracy = getTargetAccuracy(scoreProcessor, animated: false);

            const Easing accuracy_transform_easing = Easing.OutQuint;
            const double accuracy_transform_duration = ArgonTotalScorePanel.TotalScoreCounter.ROLLING_DURATION - ACCURACY_TRANSFORM_DELAY;

            const double score_tick_debounce_rate_start = 18f;
            const double score_tick_debounce_rate_end = 300f;
            const double score_tick_volume_start = 0.6f;
            const double score_tick_volume_end = 1.0f;

            gradedCircles.Progress = 0;
            glowLayer.FadeOut();
            rankSprite.FadeOut();
            rankSpriteAdditive.FadeOut()
                              .ScaleTo(Vector2.One);
            sampleContainer.TransformBindableTo(tickPlaybackRate, score_tick_debounce_rate_start);
            scoreTickSound.FrequencyTo(1);

            using (BeginAbsoluteSequence(startTime))
            {
                const double swoosh_pre_delay = 443f;
                const double swoosh_volume = 0.4f;

                this.Delay(swoosh_pre_delay).Schedule(() =>
                {
                    swooshUpSound.VolumeTo(swoosh_volume);
                    swooshUpSound.Play();
                });
            }

            using (BeginAbsoluteSequence(startTime + ACCURACY_TRANSFORM_DELAY))
            {
                gradedCircles.TransformTo(nameof(GradedCirclesV2.Progress), initialAccuracy, accuracy_transform_duration, accuracy_transform_easing);

                Schedule(() =>
                {
                    sampleContainer.TransformBindableTo(tickPlaybackRate, score_tick_debounce_rate_end, accuracy_transform_duration, Easing.OutSine);

                    scoreTickSound.FrequencyTo(1 + initialAccuracy, accuracy_transform_duration, Easing.OutSine);
                    scoreTickSound.VolumeTo(score_tick_volume_start).Then().VolumeTo(score_tick_volume_end, accuracy_transform_duration, Easing.OutSine);

                    isTicking = true;
                });
            }

            if (score.Value.Rank != ScoreRank.F)
            {
                int index = 0;

                foreach (var scoreRank in Enum.GetValues<ScoreRank>().Where(r => r <= score.Value.Rank && r != ScoreRank.F && r != ScoreRank.XH && r != ScoreRank.SH))
                {
                    using (BeginAbsoluteSequence(
                               startTime + ACCURACY_TRANSFORM_DELAY + inverseEasing(accuracy_transform_easing, Math.Min(1 - VIRTUAL_SS_PERCENTAGE, scoreProcessor.AccuracyCutoffFromRank(scoreRank)) / initialAccuracy) * accuracy_transform_duration))
                    {
                        Schedule(() =>
                        {
                            var dink = scoreRank != ScoreRank.X ? badgeTickSound : badgeMaxSound;

                            dink.FrequencyTo(1 + index++ * 0.05);
                            dink.Play();
                        });
                    }
                }
            }

            using (BeginAbsoluteSequence(startTime + ArgonTotalScorePanel.TotalScoreCounter.ROLLING_DURATION))
            {
                if (score.Value.Rank > ScoreRank.C)
                {
                    glowLayer.FadeIn()
                             .FlashColour(ColourInfo.GradientHorizontal(
                                 OsuColour.ForRank(score.Value.Rank).Opacity(0.0f),
                                 OsuColour.ForRank(score.Value.Rank).Opacity(0.7f)
                             ), 1000, Easing.OutSine);
                    rankSprite.FadeIn();
                    rankSpriteAdditive.FadeOutFromOne(500, Easing.OutQuint)
                                      .ScaleTo(new Vector2(1.2f), 500, Easing.OutQuint);
                }
                else
                {
                    rankSprite.FadeIn(300, Easing.OutQuint);
                    glowLayer.FadeIn(300, Easing.OutQuint);
                }

                PoolableSkinnableSample? rankImpactSound = null;

                switch (score.Value.Rank)
                {
                    case ScoreRank.D:
                    case ScoreRank.C:
                        rankImpactSound = rankImpactFailSound;
                        break;

                    case ScoreRank.B:
                    case ScoreRank.A:
                    case ScoreRank.S:
                    case ScoreRank.SH:
                        rankImpactSound = rankImpactPassSound;
                        break;

                    case ScoreRank.X:
                    case ScoreRank.XH:
                        rankImpactSound = rankImpactPassSSSound;
                        break;
                }

                Schedule(() =>
                {
                    isTicking = false;
                    rankImpactSound?.Play();
                });
            }

            if (initialAccuracy != finalAccuracy)
            {
                using (BeginAbsoluteSequence(startTime + ArgonTotalScorePanel.TotalScoreCounter.ROLLING_DURATION))
                {
                    const double flash_duration = 2000;
                    rankSprite.FlashColour(Colour4.Red, flash_duration, Easing.Out);
                    rankSpriteAdditive.FlashColour(Colour4.Red, flash_duration, Easing.Out);
                    glowLayer.FlashColour(ColourInfo.GradientHorizontal(Colour4.Red.Opacity(0), Colour4.Red), flash_duration, Easing.Out);

                    gradedCircles.TransformTo(nameof(GradedCirclesV2.Progress), finalAccuracy, 500, Easing.OutQuint);
                }
            }

            return gradedCircles.LatestTransformEndTime;
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

        public void FinishAnimating()
        {
            gradedCircles.FinishTransforms();
            glowLayer.FinishTransforms();
            rankSprite.FinishTransforms();
            rankSpriteAdditive.FinishTransforms();

            Scheduler.CancelDelayedTasks();
            isTicking = false;
            foreach (var sample in sampleContainer)
                sample.Stop();
            sampleContainer.FinishTransforms(true);
        }

        #endregion
    }
}
