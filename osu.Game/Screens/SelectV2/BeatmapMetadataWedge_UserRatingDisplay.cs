// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMetadataWedge
    {
        private partial class UserRatingDisplay : CompositeDrawable
        {
            private readonly OsuSpriteText negativeText;
            private readonly OsuSpriteText positiveText;
            private readonly Circle backgroundBar;
            private readonly Circle positiveBar;

            public int[] Data
            {
                set
                {
                    const int rating_range = 10;

                    if (!value.Any())
                    {
                        negativeText.Text = 0.ToLocalisableString(@"N0");
                        positiveText.Text = 0.ToLocalisableString(@"N0");
                        positiveBar.ResizeWidthTo(0, 300, Easing.OutQuint);
                    }
                    else
                    {
                        var usableRange = value.Skip(1).Take(rating_range); // adjust for API returning weird empty data at 0.

                        int positiveCount = usableRange.Skip(rating_range / 2).Sum();
                        int totalCount = usableRange.Sum();

                        negativeText.Text = (totalCount - positiveCount).ToLocalisableString(@"N0");
                        positiveText.Text = positiveCount.ToLocalisableString(@"N0");
                        positiveBar.ResizeWidthTo(totalCount == 0 ? 0 : (float)positiveCount / totalCount, 300, Easing.OutQuint);
                    }
                }
            }

            public UserRatingDisplay()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0f, 2f),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = BeatmapsetsStrings.ShowStatsUserRating,
                                Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Margin = new MarginPadding { Top = 10f },
                                Children = new[]
                                {
                                    negativeText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Font = OsuFont.Style.Caption1,
                                    },
                                    positiveText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Font = OsuFont.Style.Caption1,
                                    },
                                },
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new[]
                                {
                                    backgroundBar = new Circle
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 4f,
                                    },
                                    positiveBar = new Circle
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0f,
                                        Height = 4f,
                                    },
                                },
                            }
                        },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, OverlayColourProvider colourProvider)
            {
                backgroundBar.Colour = colours.DarkOrange2;
                positiveBar.Colour = colours.Lime1;
                negativeText.Colour = colourProvider.Content2;
                positiveText.Colour = colourProvider.Content2;
            }
        }
    }
}
