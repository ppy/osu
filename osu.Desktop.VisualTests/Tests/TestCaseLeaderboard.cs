// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Modes;
using osu.Game.Users;
using osu.Game.Modes.Osu;

namespace osu.Desktop.VisualTests
{
    class TestCaseLeaderboard : TestCase
    {
        public override string Description => @"From song select";

        private Leaderboard leaderboard;

        private void newScores()
        {
            var scores = new[]
            {
                new Score
                {
                    Rank = ScoreRank.SSPlus,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        Region = new Region
                        {
                            FullName = @"Spain",
                            FlagName = @"ES",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.SS,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 4608074,
                        Username = @"Skycries",
                        Region = new Region
                        {
                            FullName = @"Brazil",
                            FlagName = @"BR",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.SPlus,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 1014222,
                        Username = @"eLy",
                        Region = new Region
                        {
                            FullName = @"Japan",
                            FlagName = @"JP",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.S,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 1541390,
                        Username = @"Toukai",
                        Region = new Region
                        {
                            FullName = @"Canada",
                            FlagName = @"CA",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.A,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 2243452,
                        Username = @"Satoruu",
                        Region = new Region
                        {
                            FullName = @"Venezuela",
                            FlagName = @"VE",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.B,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 2705430,
                        Username = @"Mooha",
                        Region = new Region
                        {
                            FullName = @"France",
                            FlagName = @"FR",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.C,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 7151382,
                        Username = @"Mayuri Hana",
                        Region = new Region
                        {
                            FullName = @"Thailand",
                            FlagName = @"TH",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.D,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 2051389,
                        Username = @"FunOrange",
                        Region = new Region
                        {
                            FullName = @"Canada",
                            FlagName = @"CA",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.F,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 6169483,
                        Username = @"-Hebel-",
                        Region = new Region
                        {
                            FullName = @"Mexico",
                            FlagName = @"MX",
                        },
                    },
                },
                new Score
                {
                    Rank = ScoreRank.F,
                    Accuracy = 100,
                    MaxCombo = 244,
                    TotalScore = 1707827,
                    Mods = new Mod[] { new OsuModHidden(), new OsuModHardRock(), },
                    User = new User
                    {
                        Id = 6702666,
                        Username = @"prhtnsm",
                        Region = new Region
                        {
                            FullName = @"Germany",
                            FlagName = @"DE",
                        },
                    },
                },
            };

            leaderboard.Scores = scores;
        }

        public override void Reset()
        {
            base.Reset();

            Add(leaderboard = new Leaderboard
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
            });

            AddButton(@"New Scores", () => newScores());
            newScores();
        }
    }
}
