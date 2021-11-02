// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osuTK;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCard : OsuClickableContainer
    {
        public const float TRANSITION_DURATION = 400;

        private const float width = 408;
        private const float height = 100;
        private const float corner_radius = 10;

        private readonly APIBeatmapSet beatmapSet;

        private UpdateableOnlineBeatmapSetCover leftCover;
        private FillFlowContainer iconArea;

        private Container mainContent;
        private BeatmapCardContentBackground mainContentBackground;

        private GridContainer titleContainer;
        private GridContainer artistContainer;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        public BeatmapCard(APIBeatmapSet beatmapSet)
            : base(HoverSampleSet.Submit)
        {
            this.beatmapSet = beatmapSet;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = width;
            Height = height;
            CornerRadius = corner_radius;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3
                },
                new Container
                {
                    Name = @"Left (icon) area",
                    Size = new Vector2(height),
                    Children = new Drawable[]
                    {
                        leftCover = new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.List)
                        {
                            RelativeSizeAxes = Axes.Both,
                            OnlineInfo = beatmapSet
                        },
                        iconArea = new FillFlowContainer
                        {
                            Margin = new MarginPadding(5),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(1)
                        }
                    }
                },
                mainContent = new Container
                {
                    Name = @"Main content",
                    X = height - corner_radius,
                    Height = height,
                    CornerRadius = corner_radius,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        mainContentBackground = new BeatmapCardContentBackground(beatmapSet)
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Horizontal = 10,
                                Vertical = 4
                            },
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
                        new FillFlowContainer
                        {
                            Name = @"Bottom content",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding
                            {
                                Horizontal = 10,
                                Vertical = 4
                            },
                            Spacing = new Vector2(4, 0),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Children = new Drawable[]
                            {
                                new BeatmapSetOnlineStatusPill
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Status = beatmapSet.Status,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft
                                },
                                new DifficultySpectrumDisplay(beatmapSet)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    DotSize = new Vector2(6, 12)
                                }
                            }
                        }
                    }
                }
            };

            if (beatmapSet.HasVideo)
                iconArea.Add(new IconPill(FontAwesome.Solid.Film));

            if (beatmapSet.HasStoryboard)
                iconArea.Add(new IconPill(FontAwesome.Solid.Image));

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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
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

        private void updateState()
        {
            float targetWidth = width - height;
            if (IsHovered)
                targetWidth -= 20;

            mainContent.ResizeWidthTo(targetWidth, TRANSITION_DURATION, Easing.OutQuint);
            mainContentBackground.Dimmed.Value = IsHovered;

            leftCover.FadeColour(IsHovered ? OsuColour.Gray(0.2f) : Color4.White, TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
