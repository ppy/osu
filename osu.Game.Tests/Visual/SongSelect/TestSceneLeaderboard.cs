// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    [Description("PlaySongSelect leaderboard")]
    public class TestSceneLeaderboard : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Placeholder),
            typeof(MessagePlaceholder),
            typeof(RetrievalFailurePlaceholder),
        };

        private readonly FailableLeaderboard leaderboard;

        public TestSceneLeaderboard()
        {
            Add(leaderboard = new FailableLeaderboard
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
                Scope = BeatmapLeaderboardScope.Global,
            });

            AddStep(@"New Scores", newScores);
            AddStep(@"Empty Scores", () => leaderboard.SetRetrievalState(PlaceholderState.NoScores));
            AddStep(@"Network failure", () => leaderboard.SetRetrievalState(PlaceholderState.NetworkFailure));
            AddStep(@"No supporter", () => leaderboard.SetRetrievalState(PlaceholderState.NotSupporter));
            AddStep(@"Not logged in", () => leaderboard.SetRetrievalState(PlaceholderState.NotLoggedIn));
            AddStep(@"Unavailable", () => leaderboard.SetRetrievalState(PlaceholderState.Unavailable));
            AddStep(@"Ranked beatmap", rankedBeatmap);
            AddStep(@"Approved beatmap", approvedBeatmap);
            AddStep(@"Qualified beatmap", qualifiedBeatmap);
            AddStep(@"Loved beatmap", lovedBeatmap);
            AddStep(@"Pending beatmap", pendingBeatmap);
            AddStep(@"WIP beatmap", wipBeatmap);
            AddStep(@"Graveyard beatmap", graveyardBeatmap);
            AddStep(@"Unpublished beatmap", unpublishedBeatmap);
        }

        private void newScores()
        {
            var scores = new[]
            {
                new ScoreInfo
                {
                    Rank = ScoreRank.XH,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        Country = new Country
                        {
                            FullName = @"Spain",
                            FlagName = @"ES",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.X,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 4608074,
                        Username = @"Skycries",
                        Country = new Country
                        {
                            FullName = @"Brazil",
                            FlagName = @"BR",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.SH,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 1014222,
                        Username = @"eLy",
                        Country = new Country
                        {
                            FullName = @"Japan",
                            FlagName = @"JP",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.S,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 1541390,
                        Username = @"Toukai",
                        Country = new Country
                        {
                            FullName = @"Canada",
                            FlagName = @"CA",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.A,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 2243452,
                        Username = @"Satoruu",
                        Country = new Country
                        {
                            FullName = @"Venezuela",
                            FlagName = @"VE",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.B,
                    Accuracy = 0.9826,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 2705430,
                        Username = @"Mooha",
                        Country = new Country
                        {
                            FullName = @"France",
                            FlagName = @"FR",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.C,
                    Accuracy = 0.9654,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 7151382,
                        Username = @"Mayuri Hana",
                        Country = new Country
                        {
                            FullName = @"Thailand",
                            FlagName = @"TH",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    Accuracy = 0.6025,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 2051389,
                        Username = @"FunOrange",
                        Country = new Country
                        {
                            FullName = @"Canada",
                            FlagName = @"CA",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    Accuracy = 0.5140,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 6169483,
                        Username = @"-Hebel-",
                        Country = new Country
                        {
                            FullName = @"Mexico",
                            FlagName = @"MX",
                        },
                    },
                },
                new ScoreInfo
                {
                    Rank = ScoreRank.D,
                    Accuracy = 0.4222,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    //Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 6702666,
                        Username = @"prhtnsm",
                        Country = new Country
                        {
                            FullName = @"Germany",
                            FlagName = @"DE",
                        },
                    },
                },
            };

            leaderboard.Scores = scores;
        }

        private void rankedBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = BeatmapSetOnlineStatus.Ranked,
            };
        }

        private void approvedBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = BeatmapSetOnlineStatus.Approved,
            };
        }

        private void qualifiedBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = BeatmapSetOnlineStatus.Qualified,
            };
        }

        private void lovedBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = BeatmapSetOnlineStatus.Loved,
            };
        }

        private void pendingBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = BeatmapSetOnlineStatus.Pending,
            };
        }

        private void wipBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = BeatmapSetOnlineStatus.WIP,
            };
        }

        private void graveyardBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = 1113057,
                Status = BeatmapSetOnlineStatus.Graveyard,
            };
        }

        private void unpublishedBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                OnlineBeatmapID = null,
                Status = BeatmapSetOnlineStatus.None,
            };
        }

        private class FailableLeaderboard : BeatmapLeaderboard
        {
            public void SetRetrievalState(PlaceholderState state)
            {
                PlaceholderState = state;
            }
        }
    }
}
