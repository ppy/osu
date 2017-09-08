// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseLeaderboard : OsuTestCase
    {
        public override string Description => @"From song select";

        private readonly Leaderboard leaderboard;

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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
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

        public TestCaseLeaderboard()
        {
            Add(leaderboard = new Leaderboard
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
            });

            AddStep(@"New Scores", newScores);
            newScores();
        }
    }
}
