// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Direct
{
    public class DirectListPanel : DirectPanel
    {
        private const float transition_duration = 120;
        private const float horizontal_padding = 10;
        private const float vertical_padding = 5;
        private const float height = 70;

        private PlayButton playButton;
        private Box progressBar;

        protected override bool FadePlayButton => false;

        protected override PlayButton PlayButton => playButton;
        protected override Box PreviewBar => progressBar;

        public DirectListPanel(BeatmapSetInfo beatmap)
            : base(beatmap)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.CornerRadius = 5;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.25f), Color4.Black.Opacity(0.75f)),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding, Left = horizontal_padding, Right = vertical_padding },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            LayoutEasing = Easing.OutQuint,
                            LayoutDuration = transition_duration,
                            Spacing = new Vector2(10, 0),
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new Drawable[]
                                            {
                                                playButton = new PlayButton(SetInfo)
                                                {
                                                    Origin = Anchor.CentreLeft,
                                                    Anchor = Anchor.CentreLeft,
                                                    Size = new Vector2(height / 2),
                                                    FillMode = FillMode.Fit,
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Vertical,
                                                    Children = new Drawable[]
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
                                                    }
                                                },
                                            }
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
                            }
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Child = new DownloadButton(SetInfo)
                                    {
                                        Size = new Vector2(height - vertical_padding * 3),
                                        Margin = new MarginPadding { Left = vertical_padding, Right = vertical_padding },
                                    },
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new Statistic(FontAwesome.fa_play_circle, SetInfo.OnlineInfo?.PlayCount ?? 0)
                                        {
                                            Margin = new MarginPadding { Right = 1 },
                                        },
                                        new Statistic(FontAwesome.fa_heart, SetInfo.OnlineInfo?.FavouriteCount ?? 0),
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new[]
                                            {
                                                new OsuSpriteText
                                                {
                                                    Text = "mapped by ",
                                                    TextSize = 14,
                                                },
                                                new OsuSpriteText
                                                {
                                                    Text = SetInfo.Metadata.Author.Username,
                                                    TextSize = 14,
                                                    Font = @"Exo2.0-SemiBoldItalic",
                                                },
                                            },
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = SetInfo.Metadata.Source,
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            TextSize = 14,
                                            Alpha = string.IsNullOrEmpty(SetInfo.Metadata.Source) ? 0f : 1f,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                progressBar = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    BypassAutoSizeAxes = Axes.Y,
                    Size = new Vector2(0, 3),
                    Alpha = 0,
                    Colour = colours.Yellow,
                },
            });
        }
    }
}
