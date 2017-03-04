// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Game.Screens.Select.Leaderboards;
using OpenTK;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using System;
using osu.Framework.MathUtils;
using osu.Game.Modes;
using System.Collections.Generic;

namespace osu.Desktop.VisualTests
{
    class TestCaseLeaderboard : TestCase
    {
        public override string Name => @"Leaderboard";
        public override string Description => @"From song select";

        private Leaderboard leaderboard;
        private TextureStore ts;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            ts = textures;
        }

        private void newScores()
        {
            var scores = new List<LeaderboardScore>();
            for (int i = 0; i < 10; i++)
            {
                scores.Add(new LeaderboardScore
                {
                    Avatar = ts.Get(@"Online/avatar-guest"),
                    Flag = ts.Get(@"Flags/__"),
                    Name = @"ultralaserxx",
                    MaxCombo = RNG.Next(0, 3000),
                    Accuracy = Math.Round(RNG.NextDouble(0, 100), 2),
                    Score = RNG.Next(0, 1000000),
                    Mods = new Mod[] { },
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
