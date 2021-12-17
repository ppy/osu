// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osuTK;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardExtra : OsuClickableContainer
    {
        private const float width = 475;
        private const float height = 140;

        public Bindable<bool> Expanded { get; } = new BindableBool();

        private readonly APIBeatmapSet beatmapSet;
        private readonly Bindable<BeatmapSetFavouriteState> favouriteState;

        private readonly BeatmapDownloadTracker downloadTracker;

        [Cached]
        private readonly BeatmapCardContent content;

        private BeatmapCardThumbnail thumbnail = null!;
        private CollapsibleButtonContainer buttonContainer = null!;

        private GridContainer statisticsContainer = null!;

        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public BeatmapCardExtra(APIBeatmapSet beatmapSet)
            : base(HoverSampleSet.Submit)
        {
            this.beatmapSet = beatmapSet;
            favouriteState = new Bindable<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(beatmapSet.HasFavourited, beatmapSet.FavouriteCount));
            downloadTracker = new BeatmapDownloadTracker(beatmapSet);
            content = new BeatmapCardContent(height);
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay? beatmapSetOverlay)
        {
            Width = width;
            Height = height;

            FillFlowContainer leftIconArea = null!;
            GridContainer titleContainer = null!;
            GridContainer artistContainer = null!;

            InternalChild = content.With(c =>
            {
                c.MainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        downloadTracker,
                        thumbnail = new BeatmapCardThumbnail(beatmapSet)
                        {
                            Name = @"Left (icon) area",
                            Size = new Vector2(height),
                            Padding = new MarginPadding { Right = BeatmapCard.CORNER_RADIUS },
                            Child = leftIconArea = new FillFlowContainer
                            {
                                Margin = new MarginPadding(5),
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(1)
                            }
                        },
                        buttonContainer = new CollapsibleButtonContainer(beatmapSet)
                        {
                            X = height - BeatmapCard.CORNER_RADIUS,
                            Width = width - height + BeatmapCard.CORNER_RADIUS,
                            FavouriteState = { BindTarget = favouriteState },
                            ButtonsCollapsedWidth = BeatmapCard.CORNER_RADIUS,
                            ButtonsExpandedWidth = 30,
                            ButtonsPadding = new MarginPadding { Vertical = 35 },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        titleContainer = new GridContainer
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
                                                    new OsuSpriteText
                                                    {
                                                        Text = new RomanisableString(beatmapSet.TitleUnicode, beatmapSet.Title),
                                                        Font = OsuFont.Default.With(size: 22.5f, weight: FontWeight.SemiBold),
                                                        RelativeSizeAxes = Axes.X,
                                                        Truncate = true
                                                    },
                                                    Empty()
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
                                                    new OsuSpriteText
                                                    {
                                                        Text = createArtistText(),
                                                        Font = OsuFont.Default.With(size: 17.5f, weight: FontWeight.SemiBold),
                                                        RelativeSizeAxes = Axes.X,
                                                        Truncate = true
                                                    },
                                                    Empty()
                                                },
                                            }
                                        },
                                        new OsuSpriteText
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Truncate = true,
                                            Text = beatmapSet.Source,
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
                                                    d.AddUserLink(beatmapSet.Author);
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
                                                new BeatmapCardExtraInfoRow(beatmapSet)
                                            }
                                        },
                                        downloadProgressBar = new BeatmapCardDownloadProgressBar
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 6,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            State = { BindTarget = downloadTracker.State },
                                            Progress = { BindTarget = downloadTracker.Progress }
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
                    Child = new BeatmapCardDifficultyList(beatmapSet)
                };
                c.Expanded.BindTarget = Expanded;
            });

            if (beatmapSet.HasVideo)
                leftIconArea.Add(new IconPill(FontAwesome.Solid.Film) { IconSize = new Vector2(20) });

            if (beatmapSet.HasStoryboard)
                leftIconArea.Add(new IconPill(FontAwesome.Solid.Image) { IconSize = new Vector2(20) });

            if (beatmapSet.HasExplicitContent)
            {
                titleContainer.Content[0][1] = new ExplicitContentBeatmapPill
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 5 }
                };
            }

            if (beatmapSet.TrackId != null)
            {
                artistContainer.Content[0][1] = new FeaturedArtistBeatmapPill
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 5 }
                };
            }

            createStatistics();

            Action = () => beatmapSetOverlay?.FetchAndShowBeatmapSet(beatmapSet.OnlineID);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            downloadTracker.State.BindValueChanged(_ => updateState());
            Expanded.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private LocalisableString createArtistText()
        {
            var romanisableArtist = new RomanisableString(beatmapSet.ArtistUnicode, beatmapSet.Artist);
            return BeatmapsetsStrings.ShowDetailsByArtist(romanisableArtist);
        }

        private void createStatistics()
        {
            BeatmapCardStatistic withMargin(BeatmapCardStatistic original)
            {
                original.Margin = new MarginPadding { Right = 10 };
                return original;
            }

            statisticsContainer.Content[0][0] = withMargin(new FavouritesStatistic(beatmapSet)
            {
                Current = favouriteState,
            });

            statisticsContainer.Content[1][0] = withMargin(new PlayCountStatistic(beatmapSet));

            var hypesStatistic = HypesStatistic.CreateFor(beatmapSet);
            if (hypesStatistic != null)
                statisticsContainer.Content[0][1] = withMargin(hypesStatistic);

            var nominationsStatistic = NominationsStatistic.CreateFor(beatmapSet);
            if (nominationsStatistic != null)
                statisticsContainer.Content[1][1] = withMargin(nominationsStatistic);

            var dateStatistic = BeatmapCardDateStatistic.CreateFor(beatmapSet);
            if (dateStatistic != null)
                statisticsContainer.Content[0][2] = withMargin(dateStatistic);
        }

        private void updateState()
        {
            bool showDetails = IsHovered || Expanded.Value;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;

            // Scale value is intentionally chosen to fit in the spacing of listing displays, as to not overlap horizontally with adjacent cards.
            // This avoids depth issues where a hovered (scaled) card to the right of another card would be beneath the card to the left.
            content.ScaleTo(Expanded.Value ? 1.03f : 1, 500, Easing.OutQuint);

            bool showProgress = downloadTracker.State.Value == DownloadState.Downloading || downloadTracker.State.Value == DownloadState.Importing;

            idleBottomContent.FadeTo(showProgress ? 0 : 1, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            downloadProgressBar.FadeTo(showProgress ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
