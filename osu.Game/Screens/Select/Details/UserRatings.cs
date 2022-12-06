// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.Select.Details
{
    public partial class UserRatings : Container
    {
        private readonly FillFlowContainer header;
        private readonly Bar ratingsBar;
        private readonly OsuSpriteText negativeRatings, positiveRatings;
        private readonly Container graphContainer;
        private readonly BarGraph graph;

        private int[] ratings;

        public int[] Ratings
        {
            get => ratings;
            set
            {
                if (value == ratings) return;

                ratings = value;

                const int rating_range = 10;

                if (ratings == null)
                {
                    negativeRatings.Text = 0.ToLocalisableString(@"N0");
                    positiveRatings.Text = 0.ToLocalisableString(@"N0");
                    ratingsBar.Length = 0;
                    graph.Values = new float[rating_range];
                }
                else
                {
                    var usableRange = Ratings.Skip(1).Take(rating_range); // adjust for API returning weird empty data at 0.

                    int negativeCount = usableRange.Take(rating_range / 2).Sum();
                    int totalCount = usableRange.Sum();

                    negativeRatings.Text = negativeCount.ToLocalisableString(@"N0");
                    positiveRatings.Text = (totalCount - negativeCount).ToLocalisableString(@"N0");
                    ratingsBar.Length = totalCount == 0 ? 0 : (float)negativeCount / totalCount;
                    graph.Values = usableRange.Take(rating_range).Select(r => (float)r);
                }
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
                            Text = BeatmapsetsStrings.ShowStatsUserRating,
                            Font = OsuFont.GetFont(size: 12),
                            Margin = new MarginPadding { Bottom = 5 },
                        },
                        ratingsBar = new Bar
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 5,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Bottom = 10 },
                            Children = new[]
                            {
                                negativeRatings = new OsuSpriteText
                                {
                                    Text = 0.ToLocalisableString(@"N0"),
                                    Font = OsuFont.GetFont(size: 12)
                                },
                                positiveRatings = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Text = 0.ToLocalisableString(@"N0"),
                                    Font = OsuFont.GetFont(size: 12)
                                },
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = BeatmapsetsStrings.ShowStatsRatingSpread,
                            Font = OsuFont.GetFont(size: 12),
                            Margin = new MarginPadding { Bottom = 5 },
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
