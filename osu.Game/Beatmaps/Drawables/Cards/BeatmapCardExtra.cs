// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public partial class BeatmapCardExtra : BeatmapCard
    {
        protected override Drawable IdleContent => idleBottomContent;
        protected override Drawable DownloadInProgressContent => downloadProgressBar;

        private const float height = 140;

        [Cached]
        private readonly BeatmapCardContent content;

        private BeatmapCardThumbnail thumbnail = null!;
        private CollapsibleButtonContainer buttonContainer = null!;

        private GridContainer statisticsContainer = null!;

        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public BeatmapCardExtra(APIBeatmapSet beatmapSet, bool allowExpansion = true)
            : base(beatmapSet, allowExpansion)
        {
            content = new BeatmapCardContent(height);
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay? beatmapSetOverlay)
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
                                                new Dimension(GridSizeMode.AutoSize)
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
                                        new TruncatingSpriteText
                                        {
                                            Text = BeatmapSet.Source,
                                            Shadow = false,
                                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                            Colour = colourProvider.Content2
                                        },
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
                                                statisticsContainer = new GridContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    RowDimensions = new[]
                                                    {
                                                        new Dimension(GridSizeMode.AutoSize),
                                                        new Dimension(GridSizeMode.AutoSize)
                                                    },
                                                    ColumnDimensions = new[]
                                                    {
                                                        new Dimension(GridSizeMode.AutoSize),
                                                        new Dimension(GridSizeMode.AutoSize),
                                                        new Dimension()
                                                    },
                                                    Content = new[]
                                                    {
                                                        new Drawable[3],
                                                        new Drawable[3]
                                                    }
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

            createStatistics();

            Action = () => beatmapSetOverlay?.FetchAndShowBeatmapSet(BeatmapSet.OnlineID);
        }

        private LocalisableString createArtistText()
        {
            var romanisableArtist = new RomanisableString(BeatmapSet.ArtistUnicode, BeatmapSet.Artist);
            return BeatmapsetsStrings.ShowDetailsByArtist(romanisableArtist);
        }

        private void createStatistics()
        {
            BeatmapCardStatistic withMargin(BeatmapCardStatistic original)
            {
                original.Margin = new MarginPadding { Right = 10 };
                return original;
            }

            statisticsContainer.Content[0][0] = withMargin(new FavouritesStatistic(BeatmapSet)
            {
                Current = FavouriteState,
            });

            statisticsContainer.Content[1][0] = withMargin(new PlayCountStatistic(BeatmapSet));

            var hypesStatistic = HypesStatistic.CreateFor(BeatmapSet);
            if (hypesStatistic != null)
                statisticsContainer.Content[0][1] = withMargin(hypesStatistic);

            var nominationsStatistic = NominationsStatistic.CreateFor(BeatmapSet);
            if (nominationsStatistic != null)
                statisticsContainer.Content[1][1] = withMargin(nominationsStatistic);

            var dateStatistic = BeatmapCardDateStatistic.CreateFor(BeatmapSet);
            if (dateStatistic != null)
                statisticsContainer.Content[0][2] = withMargin(dateStatistic);
        }

        protected override void UpdateState()
        {
            base.UpdateState();

            bool showDetails = IsHovered || Expanded.Value;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;
        }
    }
}
