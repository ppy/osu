// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Modes;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private ScrollContainer scrollContainer;
        private FillFlowContainer<LeaderboardScore> scrollFlow;

        private IEnumerable<Score> scores;
        public IEnumerable<Score> Scores
        {
            get { return scores; }
            set
            {
                scores = value;

                int i = 0;
                if (scores == null)
                {
                    foreach (var c in scrollFlow.Children)
                        c.FadeOut(150 + i++ * 10);
                    return;
                }

                scrollFlow.Clear();

                i = 0;
                foreach (var s in scores)
                {
                    var ls = new LeaderboardScore(s, 1 + i)
                    {
                        AlwaysPresent = true,
                        State = Visibility.Hidden,
                    };
                    scrollFlow.Add(ls);

                    ls.Delay(i++ * 50, true);
                    ls.Show();
                }

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
                            Padding = new MarginPadding(5),
                        },
                    },
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            var fadeStart = scrollContainer.DrawHeight - 10;
            fadeStart += scrollContainer.IsScrolledToEnd() ? LeaderboardScore.HEIGHT : 0;

            foreach (var s in scrollFlow.Children)
            {
                var topY = scrollContainer.ScrollContent.DrawPosition.Y + s.DrawPosition.Y;
                var bottomY = topY + LeaderboardScore.HEIGHT;

                if (topY < fadeStart - LeaderboardScore.HEIGHT * 2)
                {
                    s.ColourInfo = ColourInfo.GradientVertical(Color4.White, Color4.White);
                }
                else
                {
                    s.ColourInfo = ColourInfo.GradientVertical(Color4.White.Opacity(System.Math.Min((fadeStart - topY) / LeaderboardScore.HEIGHT, 1)),
                                                           Color4.White.Opacity(System.Math.Min((fadeStart - bottomY) / LeaderboardScore.HEIGHT, 1)));
                }
            }
        }
    }
}
