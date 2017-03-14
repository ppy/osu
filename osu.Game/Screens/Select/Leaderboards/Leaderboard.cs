// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private ScrollContainer scrollContainer;
        private FillFlowContainer<LeaderboardScore> scrollFlow;

        private Score[] scores;
        public Score[] Scores
        {
            get { return scores; }
            set
            {
                if (value == scores) return;
                scores = value;

                var scoreDisplays = new List<LeaderboardScore>();
                for (int i = 0; i < value.Length; i++)
                {
                    scoreDisplays.Add(new LeaderboardScore(value[i], i + 1));
                }

                scrollFlow.Children = scoreDisplays;
                scrollContainer.ScrollTo(0f, false);
            }
        }

        public Leaderboard()
        {
            Children = new Drawable[]
            {
                scrollContainer = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollDraggerVisible = false,
                    Children = new Drawable[]
                    {
                        scrollFlow = new FillFlowContainer<LeaderboardScore>
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
}
