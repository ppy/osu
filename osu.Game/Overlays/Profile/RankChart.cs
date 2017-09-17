// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile
{
    public class RankChart : Container
    {
        private readonly SpriteText rankText, performanceText, relativeText;
        private readonly RankChartLineGraph graph;

        private readonly int[] ranks;

        private const float primary_textsize = 25, secondary_textsize = 13, padding = 10;

        private readonly User user;

        public RankChart(User user)
        {
            this.user = user;

            Padding = new MarginPadding { Vertical = padding };
            Children = new Drawable[]
            {
                rankText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = @"Exo2.0-RegularItalic",
                    TextSize = primary_textsize
                },
                relativeText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = @"Exo2.0-RegularItalic",
                    Y = 25,
                    TextSize = secondary_textsize
                },
                performanceText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Font = @"Exo2.0-RegularItalic",
                    TextSize = secondary_textsize
                },
                graph = new RankChartLineGraph
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Y = -secondary_textsize,
                    DefaultValueCount = 90,
                    BallRelease = updateRankTexts,
                    BallMove = showHistoryRankTexts
                }
            };

            ranks = user.RankHistory?.Data ?? new[] { user.Statistics.Rank };
        }

        private void updateRankTexts()
        {
            rankText.Text = user.Statistics.Rank > 0 ? $"#{user.Statistics.Rank:#,0}" : "no rank";
            performanceText.Text = user.Statistics.PP != null ? $"{user.Statistics.PP:#,0}pp" : string.Empty;
            relativeText.Text = $"{user.Country?.FullName} #{user.CountryRank:#,0}";
        }

        private void showHistoryRankTexts(int dayIndex)
        {
            rankText.Text = ranks[dayIndex] > 0 ? $"#{ranks[dayIndex]:#,0}" : "no rank";
            relativeText.Text = dayIndex == ranks.Length ? "Now" : $"{ranks.Length - dayIndex} days ago";
            //plural should be handled in a general way
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.Colour = colours.Yellow;

            if (user.Statistics.Rank > 0)
            {
                // use logarithmic coordinates
                graph.Values = ranks.Select(x => -(float)Math.Log(x));
                graph.ResetBall();
            }
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) != 0)
            {
                graph.Height = DrawHeight - padding * 2 - primary_textsize - secondary_textsize * 2;
            }

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        private class RankChartLineGraph : LineGraph
        {
            private readonly CircularContainer ball;
            private bool ballShown;

            private const double transform_duration = 100;

            public Action<int> BallMove;
            public Action BallRelease;

            public RankChartLineGraph()
            {
                Add(ball = new CircularContainer
                {
                    Size = new Vector2(8),
                    Masking = true,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both }
                    }
                });
            }

            public void ResetBall()
            {
                ball.MoveTo(new Vector2(1, GetYPosition(Values.Last())), ballShown ? transform_duration : 0, Easing.OutQuint);
                ball.Show();
                BallRelease();
                ballShown = true;
            }

            protected override bool OnMouseMove(InputState state)
            {
                if (ballShown)
                {
                    var values = (IList<float>)Values;
                    var position = ToLocalSpace(state.Mouse.NativeState.Position);
                    int count = Math.Max(values.Count, DefaultValueCount);
                    int index = (int)Math.Round(position.X / DrawWidth * (count - 1));
                    if (index >= count - values.Count)
                    {
                        int i = index + values.Count - count;
                        float y = GetYPosition(values[i]);
                        if (Math.Abs(y * DrawHeight - position.Y) <= 8f)
                        {
                            ball.MoveTo(new Vector2(index / (float)(count - 1), y), transform_duration, Easing.OutQuint);
                            BallMove(i);
                        }
                    }
                }
                return base.OnMouseMove(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                if (ballShown)
                    ResetBall();
                base.OnHoverLost(state);
            }
        }
    }
}
