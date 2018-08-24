// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;
using OpenTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class RankGraph : Container
    {
        private const float primary_textsize = 25;
        private const float secondary_textsize = 13;
        private const float padding = 10;
        private const float fade_duration = 150;
        private const int ranked_days = 88;

        private readonly SpriteText rankText, performanceText, relativeText;
        private readonly RankChartLineGraph graph;
        private readonly OsuSpriteText placeholder;

        private KeyValuePair<int, int>[] ranks;
        public Bindable<User> User = new Bindable<User>();

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
                    TextSize = 14,
                    Font = @"Exo2.0-RegularItalic",
                },
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
                    Height = 75,
                    Y = -secondary_textsize,
                    Alpha = 0,
                }
            };

            graph.OnBallMove += showHistoryRankTexts;

            User.ValueChanged += userChanged;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.Colour = colours.Yellow;
        }

        private void userChanged(User user)
        {
            placeholder.FadeIn(fade_duration, Easing.Out);

            if (user?.Statistics?.Ranks.Global == null)
            {
                rankText.Text = string.Empty;
                performanceText.Text = string.Empty;
                relativeText.Text = string.Empty;
                graph.FadeOut(fade_duration, Easing.Out);
                ranks = null;
                return;
            }

            int[] userRanks = user.RankHistory?.Data ?? new[] { user.Statistics.Ranks.Global.Value };
            ranks = userRanks.Select((x, index) => new KeyValuePair<int, int>(index, x)).Where(x => x.Value != 0).ToArray();

            if (ranks.Length > 1)
            {
                placeholder.FadeOut(fade_duration, Easing.Out);

                graph.DefaultValueCount = ranks.Length;
                graph.Values = ranks.Select(x => -(float)Math.Log(x.Value));
                graph.SetStaticBallPosition();
            }

            graph.FadeTo(ranks.Length > 1 ? 1 : 0, fade_duration, Easing.Out);

            updateRankTexts();
        }

        private void updateRankTexts()
        {
            var user = User.Value;

            performanceText.Text = user.Statistics.PP != null ? $"{user.Statistics.PP:#,0}pp" : string.Empty;
            rankText.Text = user.Statistics.Ranks.Global > 0 ? $"#{user.Statistics.Ranks.Global:#,0}" : "no rank";
            relativeText.Text = user.Country != null && user.Statistics.Ranks.Country > 0 ? $"{user.Country.FullName} #{user.Statistics.Ranks.Country:#,0}" : "no rank";
        }

        private void showHistoryRankTexts(int dayIndex)
        {
            rankText.Text = $"#{ranks[dayIndex].Value:#,0}";
            relativeText.Text = dayIndex + 1 == ranks.Length ? "Now" : $"{ranked_days - ranks[dayIndex].Key} days ago";
        }

        protected override bool OnHover(InputState state)
        {
            if (ranks?.Length > 1)
            {
                graph.UpdateBallPosition(state.Mouse.Position.X);
                graph.ShowBall();
            }
            return base.OnHover(state);
        }

        protected override bool OnMouseMove(InputState state)
        {
            if (ranks?.Length > 1)
                graph.UpdateBallPosition(state.Mouse.Position.X);

            return base.OnMouseMove(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (ranks?.Length > 1)
            {
                graph.HideBall();
                updateRankTexts();
            }

            base.OnHoverLost(state);
        }

        private class RankChartLineGraph : LineGraph
        {
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
