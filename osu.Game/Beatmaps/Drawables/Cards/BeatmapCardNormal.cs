// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osuTK;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public partial class BeatmapCardNormal : BeatmapCard
    {
        protected override Drawable IdleContent => idleBottomContent;
        protected override Drawable DownloadInProgressContent => downloadProgressBar;

        private const float height = 100;

        [Cached]
        private readonly BeatmapCardContent content;

        private BeatmapCardThumbnail thumbnail = null!;
        private CollapsibleButtonContainer buttonContainer = null!;

        private FillFlowContainer<BeatmapCardStatistic> statisticsContainer = null!;

        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public BeatmapCardNormal(APIBeatmapSet beatmapSet, bool allowExpansion = true)
            : base(beatmapSet, allowExpansion)
        {
            content = new BeatmapCardContent(height);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = WIDTH;
            Height = height;

            FillFlowContainer leftIconArea = null!;
            FillFlowContainer titleBadgeArea = null!;
            GridContainer artistContainer = null!;

            Child = content.With(c =>
            {
                c.MainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        thumbnail = new BeatmapCardThumbnail(BeatmapSet)
                        {
                            Name = @"Left (icon) area",
                            Size = new Vector2(height),
                            Padding = new MarginPadding { Right = CORNER_RADIUS },
                            Child = leftIconArea = new FillFlowContainer
                            {
                                Margin = new MarginPadding(5),
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(1)
                            }
                        },
                        buttonContainer = new CollapsibleButtonContainer(BeatmapSet)
                        {
                            X = height - CORNER_RADIUS,
                            Width = WIDTH - height + CORNER_RADIUS,
                            FavouriteState = { BindTarget = FavouriteState },
                            ButtonsCollapsedWidth = CORNER_RADIUS,
                            ButtonsExpandedWidth = 30,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.AutoSize),
                                            },
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new TruncatingSpriteText
                                                    {
                                                        Text = new RomanisableString(BeatmapSet.TitleUnicode, BeatmapSet.Title),
                                                        Font = OsuFont.Default.With(size: 22.5f, weight: FontWeight.SemiBold),
                                                    },
                                                    titleBadgeArea = new FillFlowContainer
                                                    {
                                                        Anchor = Anchor.BottomRight,
                                                        Origin = Anchor.BottomRight,
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                    }
                                                }
                                            }
                                        },
                                        artistContainer = new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            Content = new[]
                                            {
                                                new[]
                                                {
                                                    new TruncatingSpriteText
                                                    {
                                                        Text = createArtistText(),
                                                        Font = OsuFont.Default.With(size: 17.5f, weight: FontWeight.SemiBold),
                                                    },
                                                    Empty()
                                                },
                                            }
                                        },
                                        new LinkFlowContainer(s =>
                                        {
                                            s.Shadow = false;
                                            s.Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold);
                                        }).With(d =>
                                        {
                                            d.AutoSizeAxes = Axes.Both;
                                            d.Margin = new MarginPadding { Top = 2 };
                                            d.AddText("mapped by ", t => t.Colour = colourProvider.Content2);
                                            d.AddUserLink(BeatmapSet.Author);
                                        }),
                                    }
                                },
                                new Container
                                {
                                    Name = @"Bottom content",
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Children = new Drawable[]
                                    {
                                        idleBottomContent = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 3),
                                            AlwaysPresent = true,
                                            Children = new Drawable[]
                                            {
                                                statisticsContainer = new FillFlowContainer<BeatmapCardStatistic>
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10, 0),
                                                    Alpha = 0,
                                                    AlwaysPresent = true,
                                                    ChildrenEnumerable = createStatistics()
                                                },
                                                new BeatmapCardExtraInfoRow(BeatmapSet)
                                            }
                                        },
                                        downloadProgressBar = new BeatmapCardDownloadProgressBar
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 6,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            State = { BindTarget = DownloadTracker.State },
                                            Progress = { BindTarget = DownloadTracker.Progress }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                c.ExpandedContent = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 10, Vertical = 13 },
                    Child = new BeatmapCardDifficultyList(BeatmapSet)
                };
                c.Expanded.BindTarget = Expanded;
            });

            if (BeatmapSet.HasVideo)
                leftIconArea.Add(new VideoIconPill { IconSize = new Vector2(20) });

            if (BeatmapSet.HasStoryboard)
                leftIconArea.Add(new StoryboardIconPill { IconSize = new Vector2(20) });

            if (BeatmapSet.FeaturedInSpotlight)
            {
                titleBadgeArea.Add(new SpotlightBeatmapBadge
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 5 }
                });
            }

            if (BeatmapSet.HasExplicitContent)
            {
                titleBadgeArea.Add(new ExplicitContentBeatmapBadge
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 5 }
                });
            }

            if (BeatmapSet.TrackId != null)
            {
                artistContainer.Content[0][1] = new FeaturedArtistBeatmapBadge
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 5 }
                };
            }
        }

        private LocalisableString createArtistText()
        {
            var romanisableArtist = new RomanisableString(BeatmapSet.ArtistUnicode, BeatmapSet.Artist);
            return BeatmapsetsStrings.ShowDetailsByArtist(romanisableArtist);
        }

        private IEnumerable<BeatmapCardStatistic> createStatistics()
        {
            var hypesStatistic = HypesStatistic.CreateFor(BeatmapSet);
            if (hypesStatistic != null)
                yield return hypesStatistic;

            var nominationsStatistic = NominationsStatistic.CreateFor(BeatmapSet);
            if (nominationsStatistic != null)
                yield return nominationsStatistic;

            yield return new FavouritesStatistic(BeatmapSet) { Current = FavouriteState };
            yield return new PlayCountStatistic(BeatmapSet);

            var dateStatistic = BeatmapCardDateStatistic.CreateFor(BeatmapSet);
            if (dateStatistic != null)
                yield return dateStatistic;
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            bool showDetails = IsHovered || Expanded.Value;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;

            statisticsContainer.FadeTo(showDetails ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
