// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing.Panels
{
    public class GridBeatmapPanel : BeatmapPanel
    {
        private const float horizontal_padding = 10;
        private const float vertical_padding = 5;

        private FillFlowContainer bottomPanel, statusContainer, titleContainer, artistContainer;
        private PlayButton playButton;
        private Box progressBar;

        protected override PlayButton PlayButton => playButton;
        protected override Box PreviewBar => progressBar;

        public GridBeatmapPanel(APIBeatmapSet beatmap)
            : base(beatmap)
        {
            Width = 380;
            Height = 140 + vertical_padding; // full height of all the elements plus vertical padding (autosize uses the image)
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            bottomPanel.LayoutDuration = 200;
            bottomPanel.LayoutEasing = Easing.Out;
            bottomPanel.Origin = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.CornerRadius = 4;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                bottomPanel = new FillFlowContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, vertical_padding),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = horizontal_padding, Right = horizontal_padding },
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                titleContainer = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = new RomanisableString(SetInfo.TitleUnicode, SetInfo.Title),
                                            Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold, italics: true)
                                        },
                                    }
                                },
                                artistContainer = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = new RomanisableString(SetInfo.ArtistUnicode, SetInfo.Artist),
                                            Font = OsuFont.GetFont(weight: FontWeight.Bold, italics: true)
                                        }
                                    }
                                }
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                progressBar = new Box
                                {
                                    Origin = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.X,
                                    BypassAutoSizeAxes = Axes.Both,
                                    Size = new Vector2(0, 3),
                                    Alpha = 0,
                                    Colour = colours.Yellow,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding
                                    {
                                        Top = vertical_padding,
                                        Bottom = vertical_padding,
                                        Left = horizontal_padding,
                                        Right = horizontal_padding,
                                    },
                                    Children = new Drawable[]
                                    {
                                        new LinkFlowContainer(s =>
                                        {
                                            s.Shadow = false;
                                            s.Font = OsuFont.GetFont(size: 14);
                                        }).With(d =>
                                        {
                                            d.AutoSizeAxes = Axes.Both;
                                            d.AddText("mapped by ", t => t.Colour = colours.Gray5);
                                            d.AddUserLink(SetInfo.Author);
                                        }),
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 14,
                                            Children = new[]
                                            {
                                                new OsuSpriteText
                                                {
                                                    Text = SetInfo.Source,
                                                    Font = OsuFont.GetFont(size: 14),
                                                    Shadow = false,
                                                    Colour = colours.Gray5,
                                                    Alpha = string.IsNullOrEmpty(SetInfo.Source) ? 0f : 1f,
                                                },
                                            },
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 20,
                                            Margin = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding },
                                            Spacing = new Vector2(3),
                                            Children = GetDifficultyIcons(colours),
                                        },
                                    },
                                },
                                new BeatmapPanelDownloadButton(SetInfo)
                                {
                                    Size = new Vector2(50, 30),
                                    Margin = new MarginPadding(horizontal_padding),
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                },
                            },
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding { Top = vertical_padding, Right = vertical_padding },
                    Children = new[]
                    {
                        new Statistic(FontAwesome.Solid.PlayCircle, SetInfo.PlayCount),
                        new Statistic(FontAwesome.Solid.Heart, SetInfo.FavouriteCount),
                    },
                },
                statusContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Top = 5, Left = 5 },
                    Spacing = new Vector2(5),
                },
                playButton = new PlayButton(SetInfo)
                {
                    Margin = new MarginPadding { Top = 5, Left = 10 },
                    Size = new Vector2(30),
                    Alpha = 0,
                },
            });

            if (SetInfo.HasExplicitContent)
            {
                titleContainer.Add(new ExplicitContentBeatmapPill
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 10f, Top = 2f },
                });
            }

            if (SetInfo.TrackId != null)
            {
                artistContainer.Add(new FeaturedArtistBeatmapPill
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 10f, Top = 2f },
                });
            }

            if (SetInfo.HasVideo)
            {
                statusContainer.Add(new IconPill(FontAwesome.Solid.Film));
            }

            if (SetInfo.HasStoryboard)
            {
                statusContainer.Add(new IconPill(FontAwesome.Solid.Image));
            }

            statusContainer.Add(new BeatmapSetOnlineStatusPill
            {
                AutoSizeAxes = Axes.Both,
                TextSize = 12,
                TextPadding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                Status = SetInfo.Status,
            });

            PreviewPlaying.ValueChanged += _ => updateStatusContainer();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateStatusContainer();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateStatusContainer();
        }

        private void updateStatusContainer() => statusContainer.FadeTo(IsHovered || PreviewPlaying.Value ? 0 : 1, 120, Easing.InOutQuint);
    }
}
