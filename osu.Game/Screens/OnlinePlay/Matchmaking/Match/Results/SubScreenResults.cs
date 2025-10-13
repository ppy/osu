// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results
{
    /// <summary>
    /// Final room results, during <see cref="MatchmakingStage.Ended"/>
    /// </summary>
    public partial class SubScreenResults : MatchmakingSubScreen
    {
        private const float grid_spacing = 5;

        public override PanelDisplayStyle PlayersDisplayStyle => PanelDisplayStyle.Grid;

        public override Drawable PlayersDisplayArea { get; } = new Container { RelativeSizeAxes = Axes.Both };

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private OsuSpriteText placementText = null!;
        private FillFlowContainer<PanelUserStatistic> userStatistics = null!;
        private FillFlowContainer<PanelRoomAward> roomAwards = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                Padding = new MarginPadding(5),
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Absolute, grid_spacing),
                    new Dimension(),
                },
                Content = new[]
                {
                    new[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Masking = true,
                                    CornerRadius = 5,
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = colourProvider.Background4,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding(6),
                                    Spacing = new Vector2(grid_spacing),
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Text = "How you played",
                                            Font = OsuFont.Style.Heading2,
                                            Margin = new MarginPadding { Vertical = 15 },
                                        },
                                        userStatistics = new FillFlowContainer<PanelUserStatistic>
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(grid_spacing)
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Text = "Room Awards",
                                            Font = OsuFont.Style.Heading2,
                                            Margin = new MarginPadding { Vertical = 15 },
                                        },
                                        roomAwards = new FillFlowContainer<PanelRoomAward>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Spacing = new Vector2(grid_spacing)
                                        }
                                    }
                                }
                            },
                        },
                        Empty(),
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, grid_spacing),
                                new Dimension(),
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
                                        Spacing = new Vector2(16),
                                        Children = new[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Text = "Your final placement",
                                                Font = OsuFont.Style.Heading2.With(size: 36),
                                            },
                                            placementText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Font = OsuFont.Style.Heading1.With(size: 72),
                                                UseFullGlyphHeight = false
                                            }
                                        }
                                    }
                                ],
                                null,
                                [
                                    PlayersDisplayArea,
                                ],
                            }
                        },
                    },
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
                placementText.Colour = OsuColour.Gray(1f);
                return;
            }

            int overallPlacement = state.Users[client.LocalUser!.UserID].Placement;

            placementText.Text = overallPlacement.Ordinalize(CultureInfo.CurrentCulture);
            placementText.Colour = ColourForPlacement(overallPlacement);

            int overallPoints = state.Users[client.LocalUser!.UserID].Points;
            addStatistic(overallPlacement, $"Overall position ({overallPoints} points)");

            var accuracyOrderedUsers = state.Users.Select(u => (user: u, avgAcc: u.Rounds.Select(r => r.Accuracy).DefaultIfEmpty(0).Average()))
                                            .OrderByDescending(t => t.avgAcc)
                                            .Select((t, i) => (info: t, index: i))
                                            .Single(t => t.info.user.UserId == client.LocalUser!.UserID);
            int accuracyPlacement = accuracyOrderedUsers.index + 1;
            addStatistic(accuracyPlacement, $"Overall accuracy ({accuracyOrderedUsers.info.avgAcc.FormatAccuracy()})");

            var maxComboOrderedUsers = state.Users.Select(u => (user: u, maxCombo: u.Rounds.Max(r => r.MaxCombo)))
                                            .OrderByDescending(t => t.maxCombo)
                                            .Select((t, i) => (info: t, index: i))
                                            .Single(t => t.info.user.UserId == client.LocalUser!.UserID);
            int maxComboPlacement = maxComboOrderedUsers.index + 1;
            addStatistic(maxComboPlacement, $"Best max combo ({maxComboOrderedUsers.info.maxCombo}x)");

            var bestPlacement = state.Users[client.LocalUser!.UserID].Rounds.MinBy(r => r.Placement);
            addStatistic(bestPlacement!.Placement, $"Best round placement (round {bestPlacement.Round})");

            void addStatistic(int position, string text) => userStatistics.Add(new PanelUserStatistic(position, text));
        }

        public static ColourInfo ColourForPlacement(int overallPlacement)
        {
            // for top 3 placements use special colours.
            // don't for the rest.

            switch (overallPlacement)
            {
                case 1:
                    return OsuColour.ForRankingTier(RankingTier.Gold);

                case 2:
                    return OsuColour.ForRankingTier(RankingTier.Silver);

                case 3:
                    return OsuColour.ForRankingTier(RankingTier.Bronze);

                default:
                    return OsuColour.ForRankingTier(RankingTier.Iron);
            }
        }

        private void populateRoomStatistics(MatchmakingRoomState state)
        {
            roomAwards.Clear();

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

            addAward(maxScoreUserId, "Score champ", "Highest score in a single round");

            addAward(maxAccuracyUserId, "Most accurate", "Highest accuracy in a single round");

            addAward(maxComboUserId, "Top combo", "Highest combo in a single round");

            if (maxBonusScoreUserId > 0)
                addAward(maxBonusScoreUserId, "Biggest bonus", "Biggest bonus score across all rounds");

            if (smallestScoreDifferenceUserId > 0)
                addAward(smallestScoreDifferenceUserId, "Most clutch", "Smallest winning score difference in a single round");

            if (largestScoreDifferenceUserId > 0)
                addAward(largestScoreDifferenceUserId, "Best finish", "Largest score difference in a single round");

            void addAward(int userId, string text, string description) => roomAwards.Add(new PanelRoomAward(text, description, userId));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.MatchRoomStateChanged -= onRoomStateChanged;
        }
    }
}
