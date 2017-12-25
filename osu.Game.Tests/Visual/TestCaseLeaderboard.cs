﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using osu.Framework.Allocation;
using OpenTK;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual
{
    [Description("PlaySongSelect leaderboard")]
    public class TestCaseLeaderboard : OsuTestCase
    {
        private RulesetStore rulesets;

        private readonly FailableLeaderboard leaderboard;

        public TestCaseLeaderboard()
        {
            Add(leaderboard = new FailableLeaderboard
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
                Scope = LeaderboardScope.Global,
            });

            AddStep(@"New Scores", newScores);
            AddStep(@"Empty Scores", () => leaderboard.SetRetrievalState(PlaceholderState.NoScores));
            AddStep(@"Network failure", () => leaderboard.SetRetrievalState(PlaceholderState.NetworkFailure));
            AddStep(@"No supporter", () => leaderboard.SetRetrievalState(PlaceholderState.NotSupporter));
            AddStep(@"Not logged in", () => leaderboard.SetRetrievalState(PlaceholderState.NotLoggedIn));
            AddStep(@"Unavailable", () => leaderboard.SetRetrievalState(PlaceholderState.Unavailable));
            AddStep(@"Real beatmap", realBeatmap);
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        private void newScores()
        {
            var scores = new[]
            {
                new Score
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
                new Score
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
                new Score
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
                new Score
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
                new Score
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
                new Score
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
                new Score
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
                new Score
                {
                    Rank = ScoreRank.F,
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
                new Score
                {
                    Rank = ScoreRank.F,
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
                new Score
                {
                    Rank = ScoreRank.F,
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

        private void realBeatmap()
        {
            leaderboard.Beatmap = new BeatmapInfo
            {
                StarDifficulty = 1.36,
                Version = @"BASIC",
                OnlineBeatmapID = 1113057,
                Ruleset = rulesets.GetRuleset(0),
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 4,
                    DrainRate = 6.5f,
                    OverallDifficulty = 6.5f,
                    ApproachRate = 5,
                },
                OnlineInfo = new BeatmapOnlineInfo
                {
                    Length = 115000,
                    CircleCount = 265,
                    SliderCount = 71,
                    PlayCount = 47906,
                    PassCount = 19899,
                },
                Metrics = new BeatmapMetrics
                {
                    Ratings = Enumerable.Range(0, 11),
                    Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                    Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                },
            };
        }

        private class FailableLeaderboard : Leaderboard
        {
            public void SetRetrievalState(PlaceholderState state)
            {
                PlaceholderState = state;
            }
        }
    }
}
