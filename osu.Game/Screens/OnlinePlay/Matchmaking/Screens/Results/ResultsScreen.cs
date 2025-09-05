// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Results
{
    public partial class ResultsScreen : MatchmakingSubScreen
    {
        private const float grid_spacing = 5;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private OsuSpriteText placementText = null!;
        private FillFlowContainer<UserStatisticPanel> userStatistics = null!;
        private FillFlowContainer<RoomStatisticPanel> roomStatistics = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions =
                [
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Absolute, grid_spacing),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, grid_spacing),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Absolute, 75)
                ],
                Content = new Drawable[]?[]
                {
                    [
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(grid_spacing),
                            Children = new[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Text = "Placement",
                                    Font = OsuFont.Default.With(size: 12)
                                },
                                placementText = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = OsuFont.Default.With(size: 72),
                                    UseFullGlyphHeight = false
                                }
                            }
                        }
                    ],
                    null,
                    [
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, grid_spacing),
                                new Dimension()
                            ],
                            Content = new Drawable?[][]
                            {
                                [
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(grid_spacing),
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Text = "Breakdown",
                                                Font = OsuFont.Default.With(size: 12)
                                            },
                                            userStatistics = new FillFlowContainer<UserStatisticPanel>
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(grid_spacing)
                                            }
                                        }
                                    },
                                    null,
                                    new PlayerPanelList
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    }
                                ]
                            }
                        }
                    ],
                    null,
                    [
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(grid_spacing),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Text = "Statistics",
                                    Font = OsuFont.Default.With(size: 12)
                                },
                                roomStatistics = new FillFlowContainer<RoomStatisticPanel>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(grid_spacing)
                                }
                            }
                        },
                    ],
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchRoomStateChanged += onRoomStateChanged;

            onRoomStateChanged(client.Room?.MatchState);
        }

        private void onRoomStateChanged(MatchRoomState? state) => Scheduler.Add(() =>
        {
            if (state is not MatchmakingRoomState matchmakingState || matchmakingState.Stage != MatchmakingStage.Ended)
                return;

            populateUserStatistics(matchmakingState);
            populateRoomStatistics(matchmakingState);
        });

        private void populateUserStatistics(MatchmakingRoomState state)
        {
            userStatistics.Clear();

            if (state.Users[client.LocalUser!.UserID].Rounds.Count == 0)
            {
                placementText.Text = "-";
                addStatistic("No rounds played");
                return;
            }

            int overallPlacement = state.Users[client.LocalUser!.UserID].Placement;
            int overallPoints = state.Users[client.LocalUser!.UserID].Points;
            int bestPlacement = state.Users[client.LocalUser!.UserID].Rounds.Min(r => r.Placement);
            var accuracyPlacement = state.Users.Select(u => (user: u, avgAcc: u.Rounds.Select(r => r.Accuracy).DefaultIfEmpty(0).Average()))
                                         .OrderByDescending(t => t.avgAcc)
                                         .Select((t, i) => (info: t, index: i))
                                         .Single(t => t.info.user.UserId == client.LocalUser!.UserID);

            placementText.Text = $"#{state.Users[client.LocalUser!.UserID].Placement}";
            addStatistic($"#{overallPlacement} overall ({overallPoints}pts)");
            addStatistic($"#{bestPlacement} best placement");
            addStatistic($"#{accuracyPlacement.index + 1} accuracy ({accuracyPlacement.info.avgAcc.FormatAccuracy()})");

            void addStatistic(string text)
            {
                userStatistics.Add(new UserStatisticPanel(text)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                });
            }
        }

        private void populateRoomStatistics(MatchmakingRoomState state)
        {
            roomStatistics.Clear();

            long maxScore = long.MinValue;
            int maxScoreUserId = 0;

            double maxAccuracy = double.MinValue;
            int maxAccuracyUserId = 0;

            int maxCombo = int.MinValue;
            int maxComboUserId = 0;

            long maxBonusScore = 0;
            int maxBonusScoreUserId = 0;

            long largestScoreDifference = long.MinValue;
            int largestScoreDifferenceUserId = 0;

            long smallestScoreDifference = long.MaxValue;
            int smallestScoreDifferenceUserId = 0;

            for (int round = 1; round <= state.CurrentRound; round++)
            {
                long roundHighestScore = long.MinValue;
                int roundHighestScoreUserId = 0;

                long roundLowestScore = long.MaxValue;

                foreach (MatchmakingUser user in state.Users)
                {
                    if (!user.Rounds.RoundsDictionary.TryGetValue(round, out MatchmakingRound? mmRound))
                        continue;

                    if (mmRound.TotalScore > maxScore)
                    {
                        maxScore = mmRound.TotalScore;
                        maxScoreUserId = user.UserId;
                    }

                    if (mmRound.Accuracy > maxAccuracy)
                    {
                        maxAccuracy = mmRound.Accuracy;
                        maxAccuracyUserId = user.UserId;
                    }

                    if (mmRound.MaxCombo > maxCombo)
                    {
                        maxCombo = mmRound.MaxCombo;
                        maxComboUserId = user.UserId;
                    }

                    if (mmRound.TotalScore > roundHighestScore)
                    {
                        roundHighestScore = mmRound.TotalScore;
                        roundHighestScoreUserId = user.UserId;
                    }

                    if (mmRound.TotalScore < roundLowestScore)
                        roundLowestScore = mmRound.TotalScore;
                }

                long roundScoreDifference = roundHighestScore - roundLowestScore;

                if (roundScoreDifference > 0 && roundScoreDifference > largestScoreDifference)
                {
                    largestScoreDifference = roundScoreDifference;
                    largestScoreDifferenceUserId = roundHighestScoreUserId;
                }

                if (roundScoreDifference > 0 && roundScoreDifference < smallestScoreDifference)
                {
                    smallestScoreDifference = roundScoreDifference;
                    smallestScoreDifferenceUserId = roundHighestScoreUserId;
                }
            }

            foreach (MatchmakingUser user in state.Users)
            {
                int userBonusScore = 0;

                foreach (MatchmakingRound round in user.Rounds)
                {
                    userBonusScore += round.Statistics.TryGetValue(HitResult.LargeBonus, out int bonus) ? bonus * 5 : 0;
                    userBonusScore += round.Statistics.TryGetValue(HitResult.SmallBonus, out bonus) ? bonus : 0;
                }

                if (userBonusScore > maxBonusScore)
                {
                    maxBonusScore = userBonusScore;
                    maxBonusScoreUserId = user.UserId;
                }
            }

            // Highest score - highest score across all rounds.
            addStatistic(maxScoreUserId, "Highest score");

            // Most accurate - highest accuracy across all rounds.
            addStatistic(maxAccuracyUserId, "Most accurate");

            // Most combo - highest combo across all rounds.
            addStatistic(maxComboUserId, "Most combo");

            // Most bonus - most bonus score across all rounds.
            if (maxBonusScoreUserId > 0)
                addStatistic(maxBonusScoreUserId, "Most bonus");

            // Most clutch - smallest victory in any round.
            if (smallestScoreDifferenceUserId > 0)
                addStatistic(smallestScoreDifferenceUserId, "Most clutch");

            // Best finish - largest victory in any round.
            if (largestScoreDifferenceUserId > 0)
                addStatistic(largestScoreDifferenceUserId, "Best finish");

            void addStatistic(int userId, string text)
            {
                roomStatistics.Add(new RoomStatisticPanel(text, userId)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                });
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchRoomStateChanged -= onRoomStateChanged;
        }
    }
}
