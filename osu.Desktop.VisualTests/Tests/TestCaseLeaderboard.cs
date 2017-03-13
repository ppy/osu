// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using osu.Framework.MathUtils;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Modes;
using osu.Game.Users;

namespace osu.Desktop.VisualTests
{
    class TestCaseLeaderboard : TestCase
    {
        public override string Name => @"Leaderboard";
        public override string Description => @"From song select";

        private Leaderboard leaderboard;

        private void newScores()
        {
            var scores = new List<Score>();
            for (int i = 0; i < 10; i++)
            {
                scores.Add(new Score
                {
                    Accuracy = Math.Round(RNG.NextDouble(0, 100), 2),
                    MaxCombo = RNG.Next(0, 3000),
                    TotalScore = RNG.Next(1, 1000000),
                    Mods = Ruleset.GetRuleset(PlayMode.Osu).GetModsFor(ModType.DifficultyIncrease).ToArray(),
                    User = new Game.Users.User
                    {
                        Id = 2,
                        Username = @"peppy",
                        Region = new Region
                        {
                            FullName = @"Australia",
                            Acronym = @"AUS",
                            FlagName = @"AU",
                        },
                    },
                });
            }

            leaderboard.Scores = scores.ToArray();
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
