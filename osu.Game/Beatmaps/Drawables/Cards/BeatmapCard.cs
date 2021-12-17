// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
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
    public class BeatmapCard : OsuClickableContainer
    {
        public const float TRANSITION_DURATION = 400;
        public const float CORNER_RADIUS = 10;

        public IBindable<bool> Expanded { get; }

        private const float width = 408;
        private const float height = 100;

        private readonly APIBeatmapSet beatmapSet;
        private readonly Bindable<BeatmapSetFavouriteState> favouriteState;

        private readonly BeatmapDownloadTracker downloadTracker;

        [Cached]
        private readonly BeatmapCardContent content;

        private BeatmapCardThumbnail thumbnail = null!;
        private CollapsibleButtonContainer buttonContainer = null!;

        private FillFlowContainer<BeatmapCardStatistic> statisticsContainer = null!;

        private FillFlowContainer idleBottomContent = null!;
        private BeatmapCardDownloadProgressBar downloadProgressBar = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public BeatmapCard(APIBeatmapSet beatmapSet, bool allowExpansion = true)
            : base(HoverSampleSet.Submit)
        {
            Expanded = new BindableBool { Disabled = !allowExpansion };

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
                            Padding = new MarginPadding { Right = CORNER_RADIUS },
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
                            X = height - CORNER_RADIUS,
                            Width = width - height + CORNER_RADIUS,
                            FavouriteState = { BindTarget = favouriteState },
                            ButtonsCollapsedWidth = CORNER_RADIUS,
                            ButtonsExpandedWidth = 30,
                            ButtonsPadding = new MarginPadding { Vertical = 17.5f },
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

        private IEnumerable<BeatmapCardStatistic> createStatistics()
        {
            var hypesStatistic = HypesStatistic.CreateFor(beatmapSet);
            if (hypesStatistic != null)
                yield return hypesStatistic;

            var nominationsStatistic = NominationsStatistic.CreateFor(beatmapSet);
            if (nominationsStatistic != null)
                yield return nominationsStatistic;

            yield return new FavouritesStatistic(beatmapSet) { Current = favouriteState };
            yield return new PlayCountStatistic(beatmapSet);

            var dateStatistic = BeatmapCardDateStatistic.CreateFor(beatmapSet);
            if (dateStatistic != null)
                yield return dateStatistic;
        }

        private void updateState()
        {
            bool showDetails = IsHovered || Expanded.Value;

            buttonContainer.ShowDetails.Value = showDetails;
            thumbnail.Dimmed.Value = showDetails;

            // Scale value is intentionally chosen to fit in the spacing of listing displays, as to not overlap horizontally with adjacent cards.
            // This avoids depth issues where a hovered (scaled) card to the right of another card would be beneath the card to the left.
            content.ScaleTo(Expanded.Value ? 1.03f : 1, 500, Easing.OutQuint);

            statisticsContainer.FadeTo(showDetails ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);

            bool showProgress = downloadTracker.State.Value == DownloadState.Downloading || downloadTracker.State.Value == DownloadState.Importing;

            idleBottomContent.FadeTo(showProgress ? 0 : 1, TRANSITION_DURATION, Easing.OutQuint);
            downloadProgressBar.FadeTo(showProgress ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
