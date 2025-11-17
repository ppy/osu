// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanel
    {
        public partial class CardContentBeatmap : CardContent, IHasContextMenu
        {
            public override AvatarOverlay SelectionOverlay => selectionOverlay;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private BeatmapSetOverlay? beatmapSetOverlay { get; set; }

            private readonly IBindable<DownloadState> downloadState = new Bindable<DownloadState>();
            private readonly IBindableNumber<double> downloadProgress = new BindableDouble();
            private readonly Bindable<BeatmapSetFavouriteState> favouriteState = new Bindable<BeatmapSetFavouriteState>();
            private readonly APIBeatmapSet beatmapSet;
            private readonly APIBeatmap beatmap;
            private readonly Mod[] mods;

            private BeatmapCardThumbnail thumbnail = null!;
            private CollapsibleButtonContainer buttonContainer = null!;
            private FillFlowContainer idleBottomContent = null!;
            private BeatmapCardDownloadProgressBar downloadProgressBar = null!;
            private AvatarOverlay selectionOverlay = null!;

            public CardContentBeatmap(APIBeatmap beatmap, Mod[] mods)
            {
                this.beatmap = beatmap;
                this.mods = mods;

                beatmapSet = beatmap.BeatmapSet!;
                favouriteState.Value = new BeatmapSetFavouriteState(beatmapSet.HasFavourited, beatmapSet.FavouriteCount);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                FillFlowContainer leftIconArea;
                FillFlowContainer titleBadgeArea;
                GridContainer artistContainer;

                InternalChildren = new Drawable[]
                {
                    new BeatmapDownloadTracker(beatmap.BeatmapSet!)
                    {
                        State = { BindTarget = downloadState },
                        Progress = { BindTarget = downloadProgress },
                    },
                    thumbnail = new BeatmapCardThumbnail(beatmapSet, beatmapSet, keepLoaded: true)
                    {
                        Name = @"Left (icon) area",
                        Size = new Vector2(MatchmakingSelectPanel.HEIGHT),
                        Padding = new MarginPadding { Right = BeatmapCard.CORNER_RADIUS },
                        Child = leftIconArea = new FillFlowContainer
                        {
                            Margin = new MarginPadding(4),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(1)
                        }
                    },
                    buttonContainer = new CollapsibleButtonContainer(beatmapSet, allowNavigationToBeatmap: false, keepBackgroundLoaded: true)
                    {
                        X = MatchmakingSelectPanel.HEIGHT - BeatmapCard.CORNER_RADIUS,
                        Width = BeatmapCard.WIDTH - MatchmakingSelectPanel.HEIGHT + BeatmapCard.CORNER_RADIUS,
                        FavouriteState = { BindTarget = favouriteState },
                        ButtonsCollapsedWidth = 0,
                        ButtonsExpandedWidth = 24,
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
                                                    Text = new RomanisableString(beatmapSet.TitleUnicode, beatmapSet.Title),
                                                    Font = OsuFont.Default.With(size: 18f, weight: FontWeight.SemiBold),
                                                    RelativeSizeAxes = Axes.X,
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
                                                    Text = BeatmapsetsStrings.ShowDetailsByArtist(new RomanisableString(beatmapSet.ArtistUnicode, beatmapSet.Artist)),
                                                    Font = OsuFont.Default.With(size: 14f, weight: FontWeight.SemiBold),
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                                Empty()
                                            },
                                        }
                                    },
                                    new LinkFlowContainer(s =>
                                    {
                                        s.Shadow = false;
                                        s.Font = OsuFont.GetFont(size: 11f, weight: FontWeight.SemiBold);
                                    }).With(d =>
                                    {
                                        d.AutoSizeAxes = Axes.Both;
                                        d.Margin = new MarginPadding { Top = 1 };
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
                                        Spacing = new Vector2(0, 2),
                                        AlwaysPresent = true,
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
                                                        new Container
                                                        {
                                                            Masking = true,
                                                            CornerRadius = BeatmapCard.CORNER_RADIUS,
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Children = new Drawable[]
                                                            {
                                                                new Box
                                                                {
                                                                    Colour = colours.ForStarDifficulty(beatmap.StarRating).Darken(0.8f),
                                                                    RelativeSizeAxes = Axes.Both,
                                                                },
                                                                new FillFlowContainer
                                                                {
                                                                    Padding = new MarginPadding(4),
                                                                    RelativeSizeAxes = Axes.X,
                                                                    AutoSizeAxes = Axes.Y,
                                                                    Direction = FillDirection.Horizontal,
                                                                    Spacing = new Vector2(6, 0),
                                                                    Children = new Drawable[]
                                                                    {
                                                                        new StarRatingDisplay(new StarDifficulty(beatmap.StarRating, 0), StarRatingDisplaySize.Small, animated: true)
                                                                        {
                                                                            Origin = Anchor.CentreLeft,
                                                                            Anchor = Anchor.CentreLeft,
                                                                            Scale = new Vector2(0.9f),
                                                                        },
                                                                        new TruncatingSpriteText
                                                                        {
                                                                            Text = beatmap.DifficultyName,
                                                                            Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                                                                            Anchor = Anchor.CentreLeft,
                                                                            Origin = Anchor.CentreLeft,
                                                                        },
                                                                    }
                                                                },
                                                            }
                                                        },
                                                        new ModFlowDisplay
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Scale = new Vector2(0.5f),
                                                            Margin = new MarginPadding { Left = 5 },
                                                            Current = { Value = mods }
                                                        },
                                                    },
                                                }
                                            },
                                        }
                                    },
                                    downloadProgressBar = new BeatmapCardDownloadProgressBar
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 5,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        State = { BindTarget = downloadState },
                                        Progress = { BindTarget = downloadProgress }
                                    }
                                }
                            },
                            selectionOverlay = new AvatarOverlay
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            }
                        }
                    }
                };

                if (beatmapSet.HasVideo)
                    leftIconArea.Add(new VideoIconPill { IconSize = new Vector2(16) });

                if (beatmapSet.HasStoryboard)
                    leftIconArea.Add(new StoryboardIconPill { IconSize = new Vector2(16) });

                if (beatmapSet.FeaturedInSpotlight)
                {
                    titleBadgeArea.Add(new SpotlightBeatmapBadge
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Margin = new MarginPadding { Left = 4 }
                    });
                }

                if (beatmapSet.HasExplicitContent)
                {
                    titleBadgeArea.Add(new ExplicitContentBeatmapBadge
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Margin = new MarginPadding { Left = 4 }
                    });
                }

                if (beatmapSet.TrackId != null)
                {
                    artistContainer.Content[0][1] = new FeaturedArtistBeatmapBadge
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Margin = new MarginPadding { Left = 4 }
                    };
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                downloadState.BindValueChanged(_ => updateState(), true);

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

            private void updateState()
            {
                bool showDetails = IsHovered;

                buttonContainer.ShowDetails.Value = showDetails;
                thumbnail.Dimmed.Value = showDetails;

                bool showProgress = downloadState.Value == DownloadState.Downloading || downloadState.Value == DownloadState.Importing;

                idleBottomContent.FadeTo(showProgress ? 0 : 1, 340, Easing.OutQuint);
                downloadProgressBar.FadeTo(showProgress ? 1 : 0, 340, Easing.OutQuint);
            }

            public MenuItem[] ContextMenuItems
            {
                get
                {
                    List<MenuItem> items = new List<MenuItem>
                    {
                        new OsuMenuItem(ContextMenuStrings.ViewBeatmap, MenuItemType.Highlighted, () => beatmapSetOverlay?.FetchAndShowBeatmap(beatmap.OnlineID))
                    };

                    foreach (var button in buttonContainer.Buttons)
                    {
                        if (button.Enabled.Value)
                            items.Add(new OsuMenuItem(button.TooltipText.ToSentence(), MenuItemType.Standard, () => button.TriggerClick()));
                    }

                    return items.ToArray();
                }
            }
        }
    }
}
