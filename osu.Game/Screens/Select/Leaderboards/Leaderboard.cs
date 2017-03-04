// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using osu.Game.Modes;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private FillFlowContainer<LeaderboardScoreDisplay> scrollFlow;

        private LeaderboardScore[] scores;
        public LeaderboardScore[] Scores
        {
            get { return scores; }
            set
            {
                scores = value;

                var scoreDisplays = new List<LeaderboardScoreDisplay>();
                for (int i = 0; i < value.Length; i++)
                {
                    scoreDisplays.Add(new LeaderboardScoreDisplay(value[i], i + 1));
                }

                scrollFlow.Children = scoreDisplays;
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
                        scrollFlow = new FillFlowContainer<LeaderboardScoreDisplay>
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
