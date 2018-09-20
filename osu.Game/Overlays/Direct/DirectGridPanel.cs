// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.States;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Overlays.Direct
{
    public class DirectGridPanel : DirectPanel
    {
        private const float horizontal_padding = 10;
        private const float vertical_padding = 5;

        private FillFlowContainer bottomPanel, statusContainer;
        private PlayButton playButton;
        private Box progressBar;

        protected override PlayButton PlayButton => playButton;
        protected override Box PreviewBar => progressBar;

        public DirectGridPanel(BeatmapSetInfo beatmap) : base(beatmap)
        {
            Width = 380;
            Height = 140 + vertical_padding; //full height of all the elements plus vertical padding (autosize uses the image)
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
                            Children = new[]
                            {
                                new OsuSpriteText
                                {
                                    Text = new LocalisedString((SetInfo.Metadata.TitleUnicode, SetInfo.Metadata.Title)),
                                    TextSize = 18,
                                    Font = @"Exo2.0-BoldItalic",
                                },
                                new OsuSpriteText
                                {
                                    Text = new LocalisedString((SetInfo.Metadata.ArtistUnicode, SetInfo.Metadata.Artist)),
                                    Font = @"Exo2.0-BoldItalic",
                                },
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
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new[]
                                            {
                                                new OsuSpriteText
                                                {
                                                    Text = "mapped by ",
                                                    TextSize = 14,
                                                    Shadow = false,
                                                    Colour = colours.Gray5,
                                                },
                                                new OsuSpriteText
                                                {
                                                    Text = SetInfo.Metadata.Author.Username,
                                                    TextSize = 14,
                                                    Font = @"Exo2.0-SemiBoldItalic",
                                                    Shadow = false,
                                                    Colour = colours.BlueDark,
                                                },
                                            },
                                        },
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 14,
                                            Children = new[]
                                            {
                                                new OsuSpriteText
                                                {
                                                    Text = SetInfo.Metadata.Source,
                                                    TextSize = 14,
                                                    Shadow = false,
                                                    Colour = colours.Gray5,
                                                    Alpha = string.IsNullOrEmpty(SetInfo.Metadata.Source) ? 0f : 1f,
                                                },
                                            },
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 20,
                                            Margin = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding },
                                            Children = GetDifficultyIcons(),
                                        },
                                    },
                                },
                                new DownloadButton(SetInfo)
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
                        new Statistic(FontAwesome.fa_play_circle, SetInfo.OnlineInfo?.PlayCount ?? 0)
                        {
                            Margin = new MarginPadding { Right = 1 },
                        },
                        new Statistic(FontAwesome.fa_heart, SetInfo.OnlineInfo?.FavouriteCount ?? 0),
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

            if (SetInfo.OnlineInfo?.HasVideo ?? false)
            {
                statusContainer.Add(new IconPill(FontAwesome.fa_film));
            }

            if (SetInfo.OnlineInfo?.HasStoryboard ?? false)
            {
                statusContainer.Add(new IconPill(FontAwesome.fa_image));
            }

            statusContainer.Add(new BeatmapSetOnlineStatusPill
            {
                TextSize = 12,
                TextPadding = new MarginPadding { Horizontal = 10, Vertical = 5 },
                Status = SetInfo.OnlineInfo?.Status ?? BeatmapSetOnlineStatus.None,
            });

            PreviewPlaying.ValueChanged += _ => updateStatusContainer();
        }

        protected override bool OnHover(InputState state)
        {
            updateStatusContainer();
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            base.OnHoverLost(state);
            updateStatusContainer();
        }

        private void updateStatusContainer() => statusContainer.FadeTo(IsHovered || PreviewPlaying ? 0 : 1, 120, Easing.InOutQuint);
    }
}
