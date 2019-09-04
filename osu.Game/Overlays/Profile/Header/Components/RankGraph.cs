// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class RankGraph : Container, IHasCustomTooltip
    {
        private const float secondary_textsize = 13;
        private const float padding = 10;
        private const float fade_duration = 150;
        private const int ranked_days = 88;

        private readonly RankChartLineGraph graph;
        private readonly OsuSpriteText placeholder;

        private KeyValuePair<int, int>[] ranks;
        private int dayIndex;
        public readonly Bindable<UserStatistics> Statistics = new Bindable<UserStatistics>();

        public RankGraph()
        {
            Padding = new MarginPadding { Vertical = padding };
            Children = new Drawable[]
            {
                placeholder = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "No recent plays",
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular)
                },
                graph = new RankChartLineGraph
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Y = -secondary_textsize,
                    Alpha = 0,
                }
            };

            graph.OnBallMove += i => dayIndex = i;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.LineColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Statistics.BindValueChanged(statistics => updateStatistics(statistics.NewValue), true);
        }

        private void updateStatistics(UserStatistics statistics)
        {
            placeholder.FadeIn(fade_duration, Easing.Out);

            if (statistics?.Ranks.Global == null)
            {
                graph.FadeOut(fade_duration, Easing.Out);
                ranks = null;
                return;
            }

            int[] userRanks = statistics.RankHistory?.Data ?? new[] { statistics.Ranks.Global.Value };
            ranks = userRanks.Select((x, index) => new KeyValuePair<int, int>(index, x)).Where(x => x.Value != 0).ToArray();

            if (ranks.Length > 1)
            {
                placeholder.FadeOut(fade_duration, Easing.Out);

                graph.DefaultValueCount = ranks.Length;
                graph.Values = ranks.Select(x => -(float)Math.Log(x.Value));
            }

            graph.FadeTo(ranks.Length > 1 ? 1 : 0, fade_duration, Easing.Out);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (ranks?.Length > 1)
            {
                graph.UpdateBallPosition(e.MousePosition.X);
                graph.ShowBar();
            }

            return base.OnHover(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (ranks?.Length > 1)
                graph.UpdateBallPosition(e.MousePosition.X);

            return base.OnMouseMove(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (ranks?.Length > 1)
            {
                graph.HideBar();
            }

            base.OnHoverLost(e);
        }

        private class RankChartLineGraph : LineGraph
        {
            private readonly CircularContainer movingBall;
            private readonly Container bar;
            private readonly Box ballBg;
            private readonly Box line;

            public Action<int> OnBallMove;

            public RankChartLineGraph()
            {
                Add(bar = new Container
                {
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Alpha = 0,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        line = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Y,
                            Width = 1.5f,
                        },
                        movingBall = new CircularContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(18),
                            Masking = true,
                            BorderThickness = 4,
                            RelativePositionAxes = Axes.Y,
                            Child = ballBg = new Box { RelativeSizeAxes = Axes.Both }
                        }
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                ballBg.Colour = colours.GreySeafoamDarker;
                movingBall.BorderColour = line.Colour = colours.Yellow;
            }

            public void UpdateBallPosition(float mouseXPosition)
            {
                const int duration = 200;
                int index = calculateIndex(mouseXPosition);
                Vector2 position = calculateBallPosition(index);
                movingBall.MoveToY(position.Y, duration, Easing.OutQuint);
                bar.MoveToX(position.X, duration, Easing.OutQuint);
                OnBallMove.Invoke(index);
            }

            public void ShowBar() => bar.FadeIn(fade_duration);

            public void HideBar() => bar.FadeOut(fade_duration);

            private int calculateIndex(float mouseXPosition) => (int)Math.Round(mouseXPosition / DrawWidth * (DefaultValueCount - 1));

            private Vector2 calculateBallPosition(int index)
            {
                float y = GetYPosition(Values.ElementAt(index));
                return new Vector2(index / (float)(DefaultValueCount - 1), y);
            }
        }

        public object TooltipContent
        {
            get
            {
                if (Statistics.Value?.Ranks.Global == null)
                    return null;

                var days = ranked_days - ranks[dayIndex].Key + 1;

                return new TooltipDisplayContent
                {
                    Rank = $"#{ranks[dayIndex].Value:#,##0}",
                    Time = days == 0 ? "now" : $"{days} days ago"
                };
            }
        }

        public ITooltip GetCustomTooltip() => new RankGraphTooltip();

        private class RankGraphTooltip : VisibilityContainer, ITooltip
        {
            private readonly OsuSpriteText globalRankingText, timeText;
            private readonly Box background;

            public RankGraphTooltip()
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 10;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                                        Text = "Global Ranking "
                                    },
                                    globalRankingText = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                    }
                                }
                            },
                            timeText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.GreySeafoamDark;
            }

            public bool SetContent(object content)
            {
                if (!(content is TooltipDisplayContent info))
                    return false;

                globalRankingText.Text = info.Rank;
                timeText.Text = info.Time;
                return true;
            }

            private bool instantMove = true;

            public void Move(Vector2 pos)
            {
                if (instantMove)
                {
                    Position = pos;
                    instantMove = false;
                }
                else
                    this.MoveTo(pos, 200, Easing.OutQuint);
            }

            protected override void PopIn()
            {
                instantMove |= !IsPresent;
                this.FadeIn(200, Easing.OutQuint);
            }

            protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
        }

        private class TooltipDisplayContent
        {
            public string Rank;
            public string Time;
        }
    }
}
