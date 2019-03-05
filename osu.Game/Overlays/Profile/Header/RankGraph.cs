// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;
using osuTK;

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
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Regular, italics: true)
                },
                rankText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = OsuFont.GetFont(size: primary_textsize, weight: FontWeight.Regular, italics: true),
                },
                relativeText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = OsuFont.GetFont(size: secondary_textsize, weight: FontWeight.Regular, italics: true),
                    Y = 25,
                },
                performanceText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Font = OsuFont.GetFont(size: secondary_textsize, weight: FontWeight.Regular, italics: true)
                },
                graph = new RankChartLineGraph
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 60,
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

        private void userChanged(ValueChangedEvent<User> e)
        {
            placeholder.FadeIn(fade_duration, Easing.Out);

            if (e.NewValue?.Statistics?.Ranks.Global == null)
            {
                rankText.Text = string.Empty;
                performanceText.Text = string.Empty;
                relativeText.Text = string.Empty;
                graph.FadeOut(fade_duration, Easing.Out);
                ranks = null;
                return;
            }

            int[] userRanks = e.NewValue.RankHistory?.Data ?? new[] { e.NewValue.Statistics.Ranks.Global.Value };
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

        protected override bool OnHover(HoverEvent e)
        {
            if (ranks?.Length > 1)
            {
                graph.UpdateBallPosition(e.MousePosition.X);
                graph.ShowBall();
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
                graph.HideBall();
                updateRankTexts();
            }

            base.OnHoverLost(e);
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
