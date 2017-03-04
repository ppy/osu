// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using osu.Game.Modes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using osu.Framework.MathUtils;
using System;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private FillFlowContainer scrollFlow;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            for (int i = 0; i < 10; i++)
            {
                scrollFlow.Add(new LeaderboardScoreDisplay(new LeaderboardScore
                {
                    Avatar = textures.Get(@"Online/avatar-guest"),
                    Flag = textures.Get(@"Flags/__"),
                    Name = @"ultralaserxx",
                    MaxCombo = RNG.Next(0, 3000),
                    Accuracy = Math.Round(RNG.NextDouble(0, 100), 2),
                    Score = RNG.Next(0, 1000000),
                    Mods = new Mod[] { },
                }, i + 1));
            }
        }

        public Leaderboard()
        {
            Children = new Drawable[]
            {
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollDraggerVisible = false,
                    Children = new Drawable[]
                    {
                        scrollFlow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(0f, 5f),
                        },
                    },
                },
            };
        }
    }

    public class LeaderboardScore
    {
        public Texture Avatar;
        public Texture Flag;
        public Texture Badge;
        public string Name;
        public int MaxCombo;
        public double Accuracy;
        public int Score;
        public IEnumerable<Mod> Mods;
    }
}
