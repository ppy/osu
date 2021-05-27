// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
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
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
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

        private DrawableSample scoreTickSound;
        private DrawableSample badgeTickSound;
        private DrawableSample badgeMaxSound;
        private DrawableSample swooshUpSound;
        private DrawableSample rankDImpactSound;
        private DrawableSample rankBImpactSound;
        private DrawableSample rankCImpactSound;
        private DrawableSample rankAImpactSound;
        private DrawableSample rankSImpactSound;
        private DrawableSample rankSSImpactSound;
        private DrawableSample rankDApplauseSound;
        private DrawableSample rankBApplauseSound;
        private DrawableSample rankCApplauseSound;
        private DrawableSample rankAApplauseSound;
        private DrawableSample rankSApplauseSound;
        private DrawableSample rankSSApplauseSound;

        private Bindable<double> tickPlaybackRate = new Bindable<double>();
        private double lastTickPlaybackTime;
        private bool isTicking;

        private AudioManager audioManager;

        public AccuracyCircleAudioSettings AudioSettings = new AccuracyCircleAudioSettings();

        private readonly bool withFlair;

        public AccuracyCircle(ScoreInfo score, bool withFlair)
        {
            this.score = score;
            this.withFlair = withFlair;
        }

        public void BindAudioSettings(AccuracyCircleAudioSettings audioSettings)
        {
            foreach (var (_, prop) in audioSettings.GetSettingsSourceProperties())
            {
                var targetBindable = (IBindable)prop.GetValue(AudioSettings);
                var sourceBindable = (IBindable)prop.GetValue(audioSettings);

                targetBindable?.BindTo(sourceBindable);
            }
        }

        private void loadSample(ref DrawableSample target, string sampleName, [CanBeNull] BindableDouble volumeBindable = null)
        {
            if (IsDisposed) return;

            target?.Expire();
            AddInternal(target = new DrawableSample(audioManager.Samples.Get($"Results/{sampleName}"))
            {
                Frequency = { Value = 1.0 }
            });

            if (volumeBindable != null)
                target.Volume.BindTarget = volumeBindable;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, GameHost host)
        {
            audioManager = audio;

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
                tickPlaybackRate = new Bindable<double>(AudioSettings.TickDebounceStart.Value);

                // score ticks
                AudioSettings.TickSampleName.BindValueChanged(sample => loadSample(ref scoreTickSound, sample.NewValue), true);
                AudioSettings.SwooshSampleName.BindValueChanged(sample => loadSample(ref swooshUpSound, sample.NewValue, AudioSettings.SwooshVolume), true);

                // badge sounds
                AudioSettings.BadgeSampleName.BindValueChanged(sample => loadSample(ref badgeTickSound, sample.NewValue, AudioSettings.BadgeDinkVolume), true);
                AudioSettings.BadgeMaxSampleName.BindValueChanged(sample => loadSample(ref badgeMaxSound, sample.NewValue, AudioSettings.BadgeDinkVolume), true);

                // impacts
                AudioSettings.ImpactGradeDSampleName.BindValueChanged(sample => loadSample(ref rankDImpactSound, sample.NewValue, AudioSettings.ImpactVolume), true);
                AudioSettings.ImpactGradeCSampleName.BindValueChanged(sample => loadSample(ref rankCImpactSound, sample.NewValue, AudioSettings.ImpactVolume), true);
                AudioSettings.ImpactGradeBSampleName.BindValueChanged(sample => loadSample(ref rankBImpactSound, sample.NewValue, AudioSettings.ImpactVolume), true);
                AudioSettings.ImpactGradeASampleName.BindValueChanged(sample => loadSample(ref rankAImpactSound, sample.NewValue, AudioSettings.ImpactVolume), true);
                AudioSettings.ImpactGradeSSampleName.BindValueChanged(sample => loadSample(ref rankSImpactSound, sample.NewValue, AudioSettings.ImpactVolume), true);
                AudioSettings.ImpactGradeSSSampleName.BindValueChanged(sample => loadSample(ref rankSSImpactSound, sample.NewValue, AudioSettings.ImpactVolume), true);

                // applause
                AudioSettings.ApplauseGradeDSampleName.BindValueChanged(sample => loadSample(ref rankDApplauseSound, sample.NewValue, AudioSettings.ApplauseVolume), true);
                AudioSettings.ApplauseGradeCSampleName.BindValueChanged(sample => loadSample(ref rankCApplauseSound, sample.NewValue, AudioSettings.ApplauseVolume), true);
                AudioSettings.ApplauseGradeBSampleName.BindValueChanged(sample => loadSample(ref rankBApplauseSound, sample.NewValue, AudioSettings.ApplauseVolume), true);
                AudioSettings.ApplauseGradeASampleName.BindValueChanged(sample => loadSample(ref rankAApplauseSound, sample.NewValue, AudioSettings.ApplauseVolume), true);
                AudioSettings.ApplauseGradeSSampleName.BindValueChanged(sample => loadSample(ref rankSApplauseSound, sample.NewValue, AudioSettings.ApplauseVolume), true);
                AudioSettings.ApplauseGradeSSSampleName.BindValueChanged(sample => loadSample(ref rankSSApplauseSound, sample.NewValue, AudioSettings.ApplauseVolume), true);
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

            if (!AudioSettings.PlayTicks.Value || !isTicking) return;

            bool enoughTimePassedSinceLastPlayback = Clock.CurrentTime - lastTickPlaybackTime >= tickPlaybackRate.Value;

            if (!enoughTimePassedSinceLastPlayback) return;

            scoreTickSound?.Play();
            lastTickPlaybackTime = Clock.CurrentTime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(0).Then().ScaleTo(1, APPEAR_DURATION, Easing.OutQuint);

            if (AudioSettings.PlaySwooshSound.Value)
                this.Delay(AudioSettings.SwooshPreDelay.Value).Schedule(() => swooshUpSound?.Play());

            using (BeginDelayedSequence(RANK_CIRCLE_TRANSFORM_DELAY, true))
                innerMask.FillTo(1f, RANK_CIRCLE_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

            using (BeginDelayedSequence(ACCURACY_TRANSFORM_DELAY, true))
            {
                double targetAccuracy = score.Rank == ScoreRank.X || score.Rank == ScoreRank.XH ? 1 : Math.Min(1 - virtual_ss_percentage, score.Accuracy);

                accuracyCircle.FillTo(targetAccuracy, ACCURACY_TRANSFORM_DURATION, ACCURACY_TRANSFORM_EASING);

                if (AudioSettings.PlayTicks.Value)
                {
                    scoreTickSound?.FrequencyTo(1 + (targetAccuracy * AudioSettings.TickPitchFactor.Value), ACCURACY_TRANSFORM_DURATION, AudioSettings.TickPitchEasing.Value);
                    scoreTickSound?.VolumeTo(AudioSettings.TickVolumeStart.Value).Then().VolumeTo(AudioSettings.TickVolumeEnd.Value, ACCURACY_TRANSFORM_DURATION, AudioSettings.TickVolumeEasing.Value);
                    this.TransformBindableTo(tickPlaybackRate, AudioSettings.TickDebounceEnd.Value, ACCURACY_TRANSFORM_DURATION, AudioSettings.TickRateEasing.Value);
                }

                Schedule(() =>
                {
                    if (!AudioSettings.PlayTicks.Value) return;

                    isTicking = true;
                });

                int badgeNum = 0;

                foreach (var badge in badges)
                {
                    if (badge.Accuracy > score.Accuracy)
                        continue;

                    using (BeginDelayedSequence(inverseEasing(ACCURACY_TRANSFORM_EASING, Math.Min(1 - virtual_ss_percentage, badge.Accuracy) / targetAccuracy) * ACCURACY_TRANSFORM_DURATION, true))
                    {
                        badge.Appear();
                        Schedule(() =>
                        {
                            if (badgeTickSound == null || badgeMaxSound == null || !AudioSettings.PlayBadgeSounds.Value) return;

                            if (badgeNum < (badges.Count - 1))
                            {
                                badgeTickSound.Frequency.Value = 1 + (badgeNum++ * 0.05);
                                badgeTickSound?.Play();
                            }
                            else
                            {
                                badgeMaxSound.Frequency.Value = 1 + (badgeNum++ * 0.05);
                                badgeMaxSound?.Play();
                                isTicking = false;
                            }
                        });
                    }
                }

                using (BeginDelayedSequence(TEXT_APPEAR_DELAY, true))
                {
                    rankText.Appear();
                    Schedule(() =>
                    {
                        isTicking = false;

                        if (!AudioSettings.PlayImpact.Value) return;

                        switch (score.Rank)
                        {
                            case ScoreRank.D:
                                rankDImpactSound?.Play();
                                break;

                            case ScoreRank.C:
                                rankCImpactSound?.Play();
                                break;

                            case ScoreRank.B:
                                rankBImpactSound?.Play();
                                break;

                            case ScoreRank.A:
                                rankAImpactSound?.Play();
                                break;

                            case ScoreRank.S:
                            case ScoreRank.SH:
                                rankSImpactSound?.Play();
                                break;

                            case ScoreRank.X:
                            case ScoreRank.XH:
                                rankSSImpactSound?.Play();
                                break;
                        }
                    });

                    using (BeginDelayedSequence(AudioSettings.ApplauseDelay.Value))
                    {
                        if (!AudioSettings.PlayApplause.Value) return;

                        Schedule(() =>
                        {
                            switch (score.Rank)
                            {
                                case ScoreRank.D:
                                    rankDApplauseSound?.Play();
                                    break;

                                case ScoreRank.C:
                                    rankCApplauseSound?.Play();
                                    break;

                                case ScoreRank.B:
                                    rankBApplauseSound?.Play();
                                    break;

                                case ScoreRank.A:
                                    rankAApplauseSound?.Play();
                                    break;

                                case ScoreRank.S:
                                case ScoreRank.SH:
                                    rankSApplauseSound?.Play();
                                    break;

                                case ScoreRank.X:
                                case ScoreRank.XH:
                                    rankSSApplauseSound?.Play();
                                    break;
                            }
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

    public class AccuracyCircleAudioSettings
    {
        [SettingSource("setting")]
        public Bindable<bool> PlayTicks { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> TickSampleName { get; } = new Bindable<string>("score-tick");

        [SettingSource("setting")]
        public Bindable<bool> PlayBadgeSounds { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> BadgeSampleName { get; } = new Bindable<string>("badge-dink");

        [SettingSource("setting")]
        public Bindable<string> BadgeMaxSampleName { get; } = new Bindable<string>("badge-dink-max");

        [SettingSource("setting")]
        public Bindable<bool> PlaySwooshSound { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> SwooshSampleName { get; } = new Bindable<string>("swoosh-up");

        [SettingSource("setting")]
        public Bindable<bool> PlayImpact { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeDSampleName { get; } = new Bindable<string>("rank-impact-fail-d");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeCSampleName { get; } = new Bindable<string>("rank-impact-fail");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeBSampleName { get; } = new Bindable<string>("rank-impact-fail");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeASampleName { get; } = new Bindable<string>("rank-impact-pass");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeSSampleName { get; } = new Bindable<string>("rank-impact-pass");

        [SettingSource("setting")]
        public Bindable<string> ImpactGradeSSSampleName { get; } = new Bindable<string>("rank-impact-pass-ss");

        [SettingSource("setting")]
        public Bindable<bool> PlayApplause { get; } = new Bindable<bool>(true);

        [SettingSource("setting")]
        public BindableDouble ApplauseVolume { get; } = new BindableDouble(0.8)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble ApplauseDelay { get; } = new BindableDouble(545)
        {
            MinValue = 0,
            MaxValue = 10000,
            Precision = 1
        };

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeDSampleName { get; } = new Bindable<string>("applause-d");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeCSampleName { get; } = new Bindable<string>("applause-c");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeBSampleName { get; } = new Bindable<string>("applause-b");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeASampleName { get; } = new Bindable<string>("applause-a");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeSSampleName { get; } = new Bindable<string>("applause-s");

        [SettingSource("setting")]
        public Bindable<string> ApplauseGradeSSSampleName { get; } = new Bindable<string>("applause-s");

        [SettingSource("setting")]
        public BindableDouble TickPitchFactor { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 3,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble TickDebounceStart { get; } = new BindableDouble(18)
        {
            MinValue = 1,
            MaxValue = 100
        };

        [SettingSource("setting")]
        public BindableDouble TickDebounceEnd { get; } = new BindableDouble(300)
        {
            MinValue = 100,
            MaxValue = 1000
        };

        [SettingSource("setting")]
        public BindableDouble SwooshPreDelay { get; } = new BindableDouble(443)
        {
            MinValue = -1000,
            MaxValue = 1000
        };

        [SettingSource("setting")]
        public Bindable<Easing> TickRateEasing { get; } = new Bindable<Easing>(Easing.OutSine);

        [SettingSource("setting")]
        public Bindable<Easing> TickPitchEasing { get; } = new Bindable<Easing>(Easing.OutSine);

        [SettingSource("setting")]
        public Bindable<Easing> TickVolumeEasing { get; } = new Bindable<Easing>(Easing.OutSine);

        [SettingSource("setting")]
        public BindableDouble TickVolumeStart { get; } = new BindableDouble(0.6)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble TickVolumeEnd { get; } = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble ImpactVolume { get; } = new BindableDouble(1.0)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble BadgeDinkVolume { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };

        [SettingSource("setting")]
        public BindableDouble SwooshVolume { get; } = new BindableDouble(0.4)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.1
        };
    }
}
