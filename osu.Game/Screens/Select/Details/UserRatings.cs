﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System.Linq;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select.Details
{
    public class UserRatings : Container
    {
        private readonly FillFlowContainer header;
        private readonly Bar ratingsBar;
        private readonly OsuSpriteText negativeRatings, positiveRatings;
        private readonly Container graphContainer;
        private readonly BarGraph graph;

        private BeatmapMetrics metrics;
        public BeatmapMetrics Metrics
        {
            get { return metrics; }
            set
            {
                if (value == metrics) return;
                metrics = value;

                const int rating_range = 10;

                var ratings = Metrics.Ratings.Skip(1).Take(rating_range); // adjust for API returning weird empty data at 0.

                var negativeCount = ratings.Take(rating_range / 2).Sum();
                var totalCount = ratings.Sum();

                negativeRatings.Text = negativeCount.ToString();
                positiveRatings.Text = (totalCount - negativeCount).ToString();
                ratingsBar.Length = totalCount == 0 ? 0 : (float)negativeCount / totalCount;
                graph.Values = ratings.Take(rating_range).Select(r => (float)r);
            }
        }

        public UserRatings()
        {
            Children = new Drawable[]
            {
                header = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "User Rating",
                            TextSize = 13,
                        },
                        ratingsBar = new Bar
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 5,
                            Margin = new MarginPadding { Top = 5 },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new[]
                            {
                                negativeRatings = new OsuSpriteText
                                {
                                    Text = "0",
                                    TextSize = 13,
                                },
                                positiveRatings = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Text = @"0",
                                    TextSize = 13,
                                },
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Rating Spread",
                            TextSize = 13,
                            Margin = new MarginPadding { Top = 10, Bottom = 5 },
                        },
                    },
                },
                graphContainer = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Child = graph = new BarGraph
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ratingsBar.BackgroundColour = colours.Green;
            ratingsBar.AccentColour = colours.Yellow;
            graph.Colour = colours.BlueDark;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            graphContainer.Padding = new MarginPadding { Top = header.DrawHeight };
        }
    }
}
