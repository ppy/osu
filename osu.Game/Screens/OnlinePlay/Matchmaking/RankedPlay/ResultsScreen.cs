// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen : RankedPlaySubScreen
    {
        protected override LocalisableString StageHeading => "Results";
        protected override LocalisableString StageCaption => string.Empty;

        public override bool ShowBeatmapBackground => true;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> globalRuleset { get; set; } = null!;

        private LoadingSpinner loadingSpinner = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerPieceVisibility.Value = Visibility.Hidden;

            AddInternal(loadingSpinner = new LoadingSpinner
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loadingSpinner.Show();

            queryScores().FireAndForget();
        }

        private async Task queryScores()
        {
            try
            {
                if (client.Room == null)
                    return;

                Task<APIBeatmap?> beatmapTask = beatmapLookupCache.GetBeatmapAsync(client.Room.CurrentPlaylistItem.BeatmapID);
                TaskCompletionSource<List<MultiplayerScore>> scoreTask = new TaskCompletionSource<List<MultiplayerScore>>();

                var request = new IndexPlaylistScoresRequest(client.Room.RoomID, client.Room.Settings.PlaylistItemId);
                request.Success += req => scoreTask.SetResult(req.Scores);
                request.Failure += scoreTask.SetException;
                api.Queue(request);

                await Task.WhenAll(beatmapTask, scoreTask.Task).ConfigureAwait(false);

                APIBeatmap? apiBeatmap = beatmapTask.GetResultSafely();
                List<MultiplayerScore> apiScores = scoreTask.Task.GetResultSafely();

                if (apiBeatmap == null)
                    return;

                // Reference: PlaylistItemResultsScreen
                setScores(apiScores.Select(s => s.CreateScoreInfo(scoreManager, rulesets, new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(apiBeatmap.Difficulty),
                    Metadata =
                    {
                        Artist = apiBeatmap.Metadata.Artist,
                        Title = apiBeatmap.Metadata.Title,
                        Author = new RealmUser
                        {
                            Username = apiBeatmap.Metadata.Author.Username,
                            OnlineID = apiBeatmap.Metadata.Author.OnlineID,
                        }
                    },
                    DifficultyName = apiBeatmap.DifficultyName,
                    StarRating = apiBeatmap.StarRating,
                    Length = apiBeatmap.Length,
                    BPM = apiBeatmap.BPM
                })).ToArray());
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to load scores for playlist item.");
                throw;
            }
            finally
            {
                Scheduler.Add(() => loadingSpinner.Hide());
            }
        }

        [Resolved]
        private RankedPlayMatchInfo matchInfo { get; set; } = null!;

        private void setScores(ScoreInfo[] scores) => Scheduler.Add(() =>
        {
            int playerId = api.LocalUser.Value.OnlineID;
            int opponentId = matchInfo.RoomState.Users.Keys.Single(it => it != playerId);

            ScoreInfo playerScore = scores.SingleOrDefault(s => s.UserID == playerId) ?? new ScoreInfo
            {
                Rank = ScoreRank.F,
                Ruleset = globalRuleset.Value,
                User = new APIUser { Id = playerId }
            };

            ScoreInfo opponentScore = scores.SingleOrDefault(s => s.UserID == opponentId) ?? new ScoreInfo
            {
                Rank = ScoreRank.F,
                Ruleset = globalRuleset.Value,
                User = new APIUser { Id = opponentId }
            };

            AddInternal(new ResultScreenContent
            {
                RelativeSizeAxes = Axes.Both,
                // A little bit of room for the countdown timer...
                Margin = new MarginPadding { Top = 45 },
                PlayerScore = playerScore,
                OpponentScore = opponentScore,
                PlayerDamageInfo = matchInfo.RoomState.Users[playerId].DamageInfo!,
                OpponentDamageInfo = matchInfo.RoomState.Users[opponentId].DamageInfo!,
            });
        });

        private partial class ResultScreenContent : CompositeDrawable
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

            [BackgroundDependencyLoader]
            private void load()
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
                        new RankedPlayCornerPiece(RankedPlayColourScheme.Blue, Anchor.BottomLeft)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            State = { BindTarget = cornerPieceVisibility },
                            Child = playerUserDisplay = new RankedPlayUserDisplay(PlayerScore.UserID, Anchor.BottomLeft, RankedPlayColourScheme.Blue)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Health = { Value = PlayerDamageInfo.OldLife }
                            }
                        },
                        new RankedPlayCornerPiece(RankedPlayColourScheme.Red, Anchor.BottomRight)
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            State = { BindTarget = cornerPieceVisibility },
                            Child = opponentUserDisplay = new RankedPlayUserDisplay(OpponentScore.UserID, Anchor.BottomRight, RankedPlayColourScheme.Red)
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
                                                playerScoreDetails = new ScoreDetails(PlayerScore, RankedPlayColourScheme.Blue)
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
                                    playerScoreBar = new ScoreBar(RankedPlayColourScheme.Blue)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Height = 0.05f,
                                        Anchor = Anchor.BottomCentre,
                                        Origin = Anchor.BottomCentre,
                                        Alpha = 0,
                                    },
                                    null,
                                    opponentScoreBar = new ScoreBar(RankedPlayColourScheme.Red)
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
                                                opponentScoreDetails = new ScoreDetails(OpponentScore, RankedPlayColourScheme.Red)
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
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                double delay = 0;

                appear(ref delay);

                animateCountersAndScoreBars(ref delay);

                showScoreInfo(ref delay);

                updateHealthBars(ref delay);
            }

            private void appear(ref double delay)
            {
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

                    Schedule(() => cornerPieceVisibility.Value = Visibility.Visible);
                }

                using (BeginDelayedSequence(900))
                {
                    panelScaffold.BottomOrnament
                                 .FadeIn(300)
                                 .ResizeWidthTo(cardSize.X - 550, 600, Easing.OutExpo);
                }

                delay += 1000;
            }

            private void animateCountersAndScoreBars(ref double delay)
            {
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

                    this.TransformBindableTo(scoreBarProgress, maxScorePercent, score_text_duration, new CubicBezierEasingFunction(easeIn: 0.4, easeOut: 1));

                    scoreBarProgress.BindValueChanged(e =>
                    {
                        playerScoreBar.Height = float.Lerp(0.05f, 1f, Math.Min(e.NewValue, playerScorePercent));
                        opponentScoreBar.Height = float.Lerp(0.05f, 1f, Math.Min(e.NewValue, opponentScorePercent));
                    });
                }

                delay += 2200;
            }

            private void updateHealthBars(ref double delay)
            {
                const double text_movement_duration = 400;

                using (BeginDelayedSequence(delay))
                {
                    Schedule(() =>
                    {
                        RankedPlayUserDisplay userDisplay =
                            PlayerScore.TotalScore > OpponentScore.TotalScore
                                ? opponentUserDisplay
                                : playerUserDisplay;

                        Vector2 screenSpacePosition = userDisplay.HealthDisplay.ScreenSpaceImpactPosition;

                        var position = flyingDamageText.Parent!.ToLocalSpace(screenSpacePosition) - flyingDamageText.AnchorPosition;

                        damageCounter.FadeOut()
                                     .Delay(200)
                                     .FadeIn(300)
                                     .ScaleTo(0.9f)
                                     .ScaleTo(1f, 300, Easing.OutElasticHalf);

                        flyingDamageText.FadeIn()
                                        .MoveTo(position, text_movement_duration, Easing.InCubic)
                                        .ScaleTo(0.75f, text_movement_duration, new CubicBezierEasingFunction(easeIn: 0.35, easeOut: 0.5))
                                        .RotateTo(12 * Math.Sign(position.X), text_movement_duration, new CubicBezierEasingFunction(easeIn: 0.35, easeOut: 0.5))
                                        .Then()
                                        .FadeOut();

                        Scheduler.AddDelayed(() =>
                        {
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
                    });
                }

                delay += 400;
            }

            private void showScoreInfo(ref double delay)
            {
                using (BeginDelayedSequence(delay))
                {
                    playerScoreDetails.FadeIn(300);
                    opponentScoreDetails.FadeIn(300);
                }

                delay += 800;
            }

            private static int numDigits(long value)
            {
                if (value <= 0)
                    return 1;

                return (int)Math.Floor(Math.Log10(value)) + 1;
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
