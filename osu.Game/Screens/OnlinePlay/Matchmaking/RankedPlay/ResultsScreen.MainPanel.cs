// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen
    {
        private partial class MainPanel : CompositeDrawable
        {
            public required ScoreInfo PlayerScore { get; init; }
            public required ScoreInfo OpponentScore { get; init; }
            public required RankedPlayDamageInfo PlayerDamageInfo { get; init; }
            public required RankedPlayDamageInfo OpponentDamageInfo { get; init; }

            [Resolved]
            private RankedPlayMatchInfo matchInfo { get; set; } = null!;

            [Resolved]
            private OsuColour colour { get; set; } = null!;

            private static Vector2 cardSize => new Vector2(950, 550);

            private readonly Bindable<Visibility> cornerPieceVisibility = new Bindable<Visibility>();
            private readonly Bindable<float> scoreBarProgress = new Bindable<float>();

            private PanelScaffold panelScaffold = null!;
            private Box flash = null!;
            private ScoreDetails playerScoreDetails = null!;
            private ScoreDetails opponentScoreDetails = null!;
            private RankedPlayScoreCounter playerScoreCounter = null!;
            private RankedPlayScoreCounter opponentScoreCounter = null!;
            private RankedPlayScoreCounter damageCounter = null!;
            private OsuSpriteText flyingDamageText = null!;
            private ScoreBar playerScoreBar = null!;
            private ScoreBar opponentScoreBar = null!;
            private OsuSpriteText roundNumber = null!;
            private RankedPlayUserDisplay playerUserDisplay = null!;
            private RankedPlayUserDisplay opponentUserDisplay = null!;

            private RankedPlayDamageInfo losingDamageInfo = null!;

            private AudioContainer sampleContainer = null!;
            private DrawableSample resultsAppearSample = null!;
            private DrawableSample dmgFlySample = null!;
            private DrawableSample dmgHitSample = null!;
            private DrawableSample hpDownSample = null!;
            private DrawableSample playerAppearSample = null!;
            private DrawableSample pseudoScoreCounterSample = null!;
            private DrawableSample scoreTickSample = null!;
            private DrawableSample gradePassSample = null!;
            private DrawableSample gradePassSsSample = null!;
            private DrawableSample gradeFailSample = null!;
            private DrawableSample gradeFailDSample = null!;
            private SampleChannel? playerScoreTickChannel;
            private SampleChannel? opponentScoreTickChannel;
            private readonly BindableDouble playerScoreTickPitch = new BindableDouble();
            private readonly BindableDouble opponentScoreTickPitch = new BindableDouble();

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                // this works under the assumption that only one player can receive damage each round
                losingDamageInfo = matchInfo.RoomState.Users
                                            .Select(it => it.Value.DamageInfo)
                                            .OfType<RankedPlayDamageInfo>()
                                            .MaxBy(it => it.Damage)!;

                AddInternal(panelScaffold = new PanelScaffold
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children =
                    [
                        new RankedPlayCornerPiece(RankedPlayColourScheme.BLUE, Anchor.BottomLeft)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            State = { BindTarget = cornerPieceVisibility },
                            Child = playerUserDisplay = new RankedPlayUserDisplay(PlayerScore.User, Anchor.BottomLeft, RankedPlayColourScheme.BLUE)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Health = { Value = PlayerDamageInfo.OldLife }
                            }
                        },
                        new RankedPlayCornerPiece(RankedPlayColourScheme.RED, Anchor.BottomRight)
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            State = { BindTarget = cornerPieceVisibility },
                            Child = opponentUserDisplay = new RankedPlayUserDisplay(OpponentScore.User, Anchor.BottomRight, RankedPlayColourScheme.RED)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Health = { Value = OpponentDamageInfo.OldLife }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 110,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Padding = new MarginPadding { Bottom = 30 },
                            Child = roundNumber = new OsuSpriteText
                            {
                                Text = $"Round {matchInfo.CurrentRound}",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 36, weight: FontWeight.Bold, typeface: Typeface.TorusAlternate),
                                Alpha = 0,
                            },
                        },
                        new GridContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = cardSize,
                            Padding = new MarginPadding { Bottom = 110, Top = 60, Horizontal = 60 },
                            ColumnDimensions =
                            [
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 40),
                                new Dimension(GridSizeMode.Absolute, 60),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(GridSizeMode.Absolute, 60),
                                new Dimension(GridSizeMode.Absolute, 40),
                                new Dimension(),
                            ],
                            Content = new Drawable?[][]
                            {
                                [
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        RowDimensions =
                                        [
                                            new Dimension(),
                                            new Dimension(GridSizeMode.AutoSize)
                                        ],
                                        Content = new Drawable[][]
                                        {
                                            [
                                                playerScoreDetails = new ScoreDetails(PlayerScore, RankedPlayColourScheme.BLUE)
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Alpha = 0,
                                                },
                                            ],
                                            [
                                                playerScoreCounter = new RankedPlayScoreCounter(numDigits(PlayerScore.TotalScore))
                                                {
                                                    Font = OsuFont.GetFont(size: 60, fixedWidth: true),
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Spacing = new Vector2(-4),
                                                    Alpha = 0,
                                                    AlwaysPresent = true,
                                                }
                                            ]
                                        }
                                    },
                                    null,
                                    playerScoreBar = new ScoreBar(RankedPlayColourScheme.BLUE)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Height = 0.05f,
                                        Anchor = Anchor.BottomCentre,
                                        Origin = Anchor.BottomCentre,
                                        Alpha = 0,
                                    },
                                    null,
                                    opponentScoreBar = new ScoreBar(RankedPlayColourScheme.RED)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Height = 0.05f,
                                        Anchor = Anchor.BottomCentre,
                                        Origin = Anchor.BottomCentre,
                                        Alpha = 0,
                                    },
                                    null,
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        RowDimensions =
                                        [
                                            new Dimension(),
                                            new Dimension(GridSizeMode.AutoSize)
                                        ],
                                        Content = new Drawable[][]
                                        {
                                            [
                                                opponentScoreDetails = new ScoreDetails(OpponentScore, RankedPlayColourScheme.RED)
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Alpha = 0,
                                                },
                                            ],
                                            [
                                                opponentScoreCounter = new RankedPlayScoreCounter(numDigits(OpponentScore.TotalScore))
                                                {
                                                    Font = OsuFont.GetFont(size: 60, fixedWidth: true),
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Spacing = new Vector2(-4),
                                                    Alpha = 0,
                                                    AlwaysPresent = true,
                                                }
                                            ]
                                        }
                                    },
                                ]
                            }
                        },
                        flash = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    ],
                    BottomOrnament =
                    {
                        Size = new Vector2(200, 60),
                        Alpha = 0,
                        Children =
                        [
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Children =
                                [
                                    damageCounter = new RankedPlayScoreCounter(numDigits(losingDamageInfo.Damage))
                                    {
                                        Font = OsuFont.GetFont(size: 36, weight: FontWeight.SemiBold, fixedWidth: true),
                                        Spacing = new Vector2(-2),
                                    },
                                    flyingDamageText = new OsuSpriteText
                                    {
                                        Text = FormattableString.Invariant($"{losingDamageInfo.Damage:N0}"),
                                        Font = OsuFont.GetFont(size: 36, weight: FontWeight.SemiBold, fixedWidth: true),
                                        Spacing = new Vector2(-2),
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        BypassAutoSizeAxes = Axes.Both,
                                        Alpha = 0,
                                    },
                                    new OsuSpriteText
                                    {
                                        BypassAutoSizeAxes = Axes.Both,
                                        Text = $"{matchInfo.RoomState.DamageMultiplier.ToStandardFormattedString(maxDecimalDigits: 1)}x",
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.Centre,
                                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 42),
                                        Rotation = 30,
                                        Alpha = 0,
                                        Colour = colour.RedLight
                                    },
                                ]
                            },
                            new OsuSpriteText
                            {
                                Text = Precision.AlmostEquals(matchInfo.RoomState.DamageMultiplier, 1)
                                    ? "Damage"
                                    : $"Damage {matchInfo.RoomState.DamageMultiplier.ToStandardFormattedString(maxDecimalDigits: 1)}x",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 22),
                            },
                        ]
                    }
                });

                AddInternal(sampleContainer = new AudioContainer
                {
                    Children = new Drawable[]
                    {
                        resultsAppearSample = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/Results/results-appear")),
                        dmgFlySample = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/Results/dmg-fly")),
                        dmgHitSample = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/Results/dmg-hit")),
                        hpDownSample = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/Results/hp-down")),
                        playerAppearSample = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/Results/players-appear")),
                        pseudoScoreCounterSample = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/Results/pseudo-score-counter")),
                        scoreTickSample = new DrawableSample(audio.Samples.Get(@"Multiplayer/Matchmaking/Ranked/Results/score-tick")),
                        gradePassSample = new DrawableSample(audio.Samples.Get(@"Results/rank-impact-pass")),
                        gradePassSsSample = new DrawableSample(audio.Samples.Get(@"Results/rank-impact-pass-ss")),
                        gradeFailSample = new DrawableSample(audio.Samples.Get(@"Results/rank-impact-fail")),
                        gradeFailDSample = new DrawableSample(audio.Samples.Get(@"Results/rank-impact-fail-d")),
                    }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                playAnimation();
            }

            private void playAnimation()
            {
                const double text_movement_duration = 400;

                double delay = 0;

                resultsAppearSample.Play();

                panelScaffold.FadeIn(100)
                             .ResizeTo(0)
                             .ResizeTo(cardSize with { Y = 30 }, 600, Easing.OutExpo)
                             // deliberately cutting this delay 300ms short so the vertical resize interrupts the horizontal one
                             .Delay(300)
                             .ResizeHeightTo(cardSize.Y, 800, Easing.OutExpo);

                flash.Delay(150).FadeOut(600, Easing.Out);

                using (BeginDelayedSequence(700))
                {
                    roundNumber.FadeIn(600);
                    playerScoreCounter.FadeIn(600);
                    opponentScoreCounter.FadeIn(600);

                    Schedule(() =>
                    {
                        cornerPieceVisibility.Value = Visibility.Visible;
                        playerAppearSample.Play();
                    });
                }

                using (BeginDelayedSequence(900))
                {
                    panelScaffold.BottomOrnament
                                 .FadeIn(300)
                                 .ResizeWidthTo(cardSize.X - 550, 600, Easing.OutExpo);
                }

                delay += 1000;

                using (BeginDelayedSequence(delay))
                {
                    const double score_text_duration = 2000;

                    playerScoreCounter.TransformValueTo(PlayerScore.TotalScore, score_text_duration - 500);
                    opponentScoreCounter.TransformValueTo(OpponentScore.TotalScore, score_text_duration - 500);

                    damageCounter.TransformValueTo(losingDamageInfo.Damage, score_text_duration - 500);

                    long maxAchievableScore = Math.Max(
                        Math.Max(PlayerScore.TotalScore, OpponentScore.TotalScore),
                        1_000_000
                    );

                    float playerScorePercent = (float)PlayerScore.TotalScore / maxAchievableScore;
                    float opponentScorePercent = (float)OpponentScore.TotalScore / maxAchievableScore;
                    float maxScorePercent = Math.Max(playerScorePercent, opponentScorePercent);

                    playerScoreBar.FadeIn(100);
                    opponentScoreBar.FadeIn(100);

                    playerScoreTickChannel ??= scoreTickSample.GetChannel();
                    playerScoreTickChannel.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH;
                    playerScoreTickChannel.Frequency.BindTarget = playerScoreTickPitch;
                    playerScoreTickPitch.Value = 0.5f;
                    playerScoreTickChannel.Looping = true;

                    opponentScoreTickChannel ??= scoreTickSample.GetChannel();
                    opponentScoreTickChannel.Balance.Value = OsuGameBase.SFX_STEREO_STRENGTH;
                    opponentScoreTickChannel.Frequency.BindTarget = opponentScoreTickPitch;
                    opponentScoreTickPitch.Value = 0.5f;
                    opponentScoreTickChannel.Looping = true;

                    Schedule(() =>
                    {
                        if (losingDamageInfo.Damage > 0)
                            pseudoScoreCounterSample.Play();

                        if (PlayerScore.TotalScore > 0)
                            playerScoreTickChannel.Play();

                        if (OpponentScore.TotalScore > 0)
                            opponentScoreTickChannel.Play();
                    });

                    this.TransformBindableTo(scoreBarProgress, maxScorePercent, score_text_duration, new CubicBezierEasingFunction(easeIn: 0.4, easeOut: 1));
                    this.TransformBindableTo(playerScoreTickPitch, 0.5f + playerScorePercent, score_text_duration, Easing.OutCubic);
                    this.TransformBindableTo(opponentScoreTickPitch, 0.5f + opponentScorePercent, score_text_duration, Easing.OutCubic);

                    // safety timeout to ensure scoreTicks don't play forever
                    Scheduler.AddDelayed(() =>
                    {
                        if (playerScoreTickChannel != null)
                            playerScoreTickChannel.Looping = false;

                        if (opponentScoreTickChannel != null)
                            opponentScoreTickChannel.Looping = false;
                    }, score_text_duration + 500);

                    scoreBarProgress.BindValueChanged(e =>
                    {
                        playerScoreBar.Height = float.Lerp(0.05f, 1f, Math.Min(e.NewValue, playerScorePercent));
                        opponentScoreBar.Height = float.Lerp(0.05f, 1f, Math.Min(e.NewValue, opponentScorePercent));

                        Schedule(() =>
                        {
                            if (playerScoreTickChannel != null && playerScoreBar.Height >= playerScorePercent)
                                playerScoreTickChannel.Looping = false;

                            if (opponentScoreTickChannel != null && opponentScoreBar.Height >= opponentScorePercent)
                                opponentScoreTickChannel.Looping = false;
                        });
                    });
                }

                delay += 2200;

                using (BeginDelayedSequence(delay))
                {
                    playerScoreDetails.FadeIn(300);
                    opponentScoreDetails.FadeIn(300);

                    Schedule(() =>
                    {
                        SampleChannel playerRankChannel = getRankSample(PlayerScore.Rank).GetChannel();
                        playerRankChannel.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH;
                        playerRankChannel.Play();

                        SampleChannel opponentRankChannel = getRankSample(OpponentScore.Rank).GetChannel();
                        opponentRankChannel.Balance.Value = OsuGameBase.SFX_STEREO_STRENGTH;
                        opponentRankChannel.Play();
                    });
                }

                delay += 800;

                bool playerTookDamage = OpponentScore.TotalScore > PlayerScore.TotalScore;
                double loserPanDirection = playerTookDamage ? -OsuGameBase.SFX_STEREO_STRENGTH : OsuGameBase.SFX_STEREO_STRENGTH;

                using (BeginDelayedSequence(delay))
                {
                    Schedule(() =>
                    {
                        RankedPlayUserDisplay userDisplay =
                            PlayerScore.TotalScore > OpponentScore.TotalScore
                                ? opponentUserDisplay
                                : playerUserDisplay;

                        Vector2 screenSpacePosition = userDisplay.HealthDisplay.ScreenSpaceImpactPosition;

                        var position1 = flyingDamageText.Parent!.ToLocalSpace(screenSpacePosition) - flyingDamageText.AnchorPosition;

                        damageCounter.FadeOut()
                                     .Delay(200)
                                     .FadeIn(300)
                                     .ScaleTo(0.9f)
                                     .ScaleTo(1f, 300, Easing.OutElasticHalf);

                        var dmgFlyChannel = dmgFlySample.GetChannel();
                        this.TransformBindableTo(dmgFlyChannel.Balance, loserPanDirection, text_movement_duration, Easing.InCubic);
                        dmgFlyChannel.Play();

                        flyingDamageText.FadeIn()
                                        .MoveTo(position1, text_movement_duration, Easing.InCubic)
                                        .ScaleTo(0.75f, text_movement_duration, new CubicBezierEasingFunction(easeIn: 0.35, easeOut: 0.5))
                                        .RotateTo(12 * Math.Sign(position1.X), text_movement_duration, new CubicBezierEasingFunction(easeIn: 0.35, easeOut: 0.5))
                                        .Then()
                                        .FadeOut();

                        Scheduler.AddDelayed(() =>
                        {
                            var dmgHitChannel = dmgHitSample.GetChannel();
                            dmgHitChannel.Balance.Value = loserPanDirection;
                            dmgHitChannel.Play();

                            userDisplay.Shake(shakeDuration: 60, shakeMagnitude: 2, maximumLength: 120);

                            for (int i = 0; i < 10; i++)
                            {
                                var particle = new DamageParticle
                                {
                                    Size = new Vector2(RNG.NextSingle(5, 15)),
                                    Origin = Anchor.Centre,
                                    Position = ToLocalSpace(screenSpacePosition),
                                    Rotation = RNG.NextSingle(0, 360),
                                    Blending = BlendingParameters.Additive,
                                };

                                AddInternal(particle);

                                particle.FadeOut(600)
                                        .ScaleTo(0, 600)
                                        .RotateTo(particle.Rotation + RNG.NextSingle(-20, 20), 600)
                                        .FadeColour(Color4.Red, 600)
                                        .Expire();
                            }
                        }, text_movement_duration);
                    });
                }

                delay += text_movement_duration;

                using (BeginDelayedSequence(delay))
                {
                    Schedule(() =>
                    {
                        playerUserDisplay.Health.Value = PlayerDamageInfo.NewLife;
                        opponentUserDisplay.Health.Value = OpponentDamageInfo.NewLife;

                        Scheduler.AddDelayed(() =>
                        {
                            var hpDecreaseChannel = hpDownSample.GetChannel();
                            hpDecreaseChannel.Balance.Value = loserPanDirection;
                            hpDecreaseChannel.Play();
                        }, 900);
                    });
                }
            }

            private DrawableSample getRankSample(ScoreRank rank)
            {
                switch (rank)
                {
                    default:
                    case ScoreRank.D:
                        return gradeFailDSample;

                    case ScoreRank.C:
                    case ScoreRank.B:
                        return gradeFailSample;

                    case ScoreRank.A:
                    case ScoreRank.S:
                    case ScoreRank.SH:
                        return gradePassSample;

                    case ScoreRank.X:
                    case ScoreRank.XH:
                        return gradePassSsSample;
                }
            }

            private static int numDigits(long value)
            {
                if (value <= 0)
                    return 1;

                return (int)Math.Floor(Math.Log10(value)) + 1;
            }

            public void StopAllSamples()
            {
                sampleContainer.Volume.Value = 0;
                playerScoreTickChannel?.Stop();
                opponentScoreTickChannel?.Stop();
            }

            private partial class DamageParticle : Triangle
            {
                private Vector2 velocity = new Vector2(RNG.NextSingle(-0.3f, 0.3f), RNG.NextSingle(-0.3f, 0.3f));

                private Vector2 gravity => new Vector2(0, 0.0002f);

                protected override void Update()
                {
                    base.Update();

                    velocity += gravity * (float)Time.Elapsed;
                    Position += velocity * (float)Time.Elapsed;
                }
            }
        }
    }
}
