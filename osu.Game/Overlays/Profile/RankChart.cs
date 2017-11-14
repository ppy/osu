// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

            int[] userRanks = user.RankHistory?.Data ?? new[] { user.Statistics.Rank };
            ranks = userRanks.SkipWhile(x => x == 0).ToArray();

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
            };

            if (ranks.Length > 0)
            {
                Add(graph = new RankChartLineGraph
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Y = -secondary_textsize,
                    DefaultValueCount = ranks.Length,
                });

                graph.OnBallMove += showHistoryRankTexts;
            }
        }

        private void updateRankTexts()
        {
            rankText.Text = user.Statistics.Rank > 0 ? $"#{user.Statistics.Rank:#,0}" : "no rank";
            performanceText.Text = user.Statistics.PP != null ? $"{user.Statistics.PP:#,0}pp" : string.Empty;
            relativeText.Text = $"{user.Country?.FullName} #{user.CountryRank:#,0}";
        }

        private void showHistoryRankTexts(int dayIndex)
        {
            rankText.Text = $"#{ranks[dayIndex]:#,0}";
            dayIndex++;
            relativeText.Text = dayIndex == ranks.Length ? "Now" : $"{ranks.Length - dayIndex} days ago";
            //plural should be handled in a general way
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (graph != null)
            {
                graph.Colour = colours.Yellow;
                // use logarithmic coordinates
                graph.Values = ranks.Select(x => -(float)Math.Log(x));
                graph.SetStaticBallPosition();
            }

            updateRankTexts();
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) != 0)
            {
                graph.Height = DrawHeight - padding * 2 - primary_textsize - secondary_textsize * 2;
            }

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        protected override bool OnHover(InputState state)
        {
            graph?.UpdateBallPosition(state.Mouse.Position.X);
            graph?.ShowBall();
            return base.OnHover(state);
        }

        protected override bool OnMouseMove(InputState state)
        {
            graph?.UpdateBallPosition(state.Mouse.Position.X);
            return base.OnMouseMove(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (graph != null)
            {
                graph.HideBall();
                updateRankTexts();
            }
            base.OnHoverLost(state);
        }

        private class RankChartLineGraph : LineGraph
        {
            private const double fade_duration = 200;

            private readonly CircularContainer staticBall;
            private readonly CircularContainer movingBall;

            public Action<int> OnBallMove;

            public RankChartLineGraph()
            {
                Add(staticBall = new CircularContainer
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(8),
                    Masking = true,
                    RelativePositionAxes = Axes.Both,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                });
                Add(movingBall = new CircularContainer
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(8),
                    Alpha = 0,
                    Masking = true,
                    RelativePositionAxes = Axes.Both,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                });
            }

            public void SetStaticBallPosition() => staticBall.Position = new Vector2(1, GetYPosition(Values.Last()));

            public void UpdateBallPosition(float mouseXPosition)
            {
                int index = calculateIndex(mouseXPosition);
                movingBall.Position = calculateBallPosition(index);
                OnBallMove.Invoke(index);
            }

            public void ShowBall() => movingBall.FadeIn(fade_duration);

            public void HideBall() => movingBall.FadeOut(fade_duration);

            private int calculateIndex(float mouseXPosition) => (int)Math.Round(mouseXPosition / DrawWidth * (DefaultValueCount - 1));

            private Vector2 calculateBallPosition(int index)
            {
                float y = GetYPosition(Values.ElementAt(index));
                return new Vector2(index / (float)(DefaultValueCount - 1), y);
            }
        }
    }
}
