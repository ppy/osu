// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Direct
{
    public class DirectListPanel : DirectPanel
    {
        private readonly float horizontal_padding = 10;
        private readonly float vertical_padding = 5;
        private readonly float height = 70;

        private readonly Sprite background;
        private readonly OsuSpriteText title, artist, mapper, source;
        private readonly Statistic playCount, favouriteCount;
        private readonly FillFlowContainer difficultyIcons;

        protected override Sprite Background => background;
        protected override OsuSpriteText Title => title;
        protected override OsuSpriteText Artist => artist;
        protected override OsuSpriteText Mapper => mapper;
        protected override OsuSpriteText Source => source;
        protected override Statistic PlayCount => playCount;
        protected override Statistic FavouriteCount => favouriteCount;
        protected override FillFlowContainer DifficultyIcons => difficultyIcons;

        public DirectListPanel()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            CornerRadius = 5;
            Masking = true;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(0f, 1f),
                Radius = 3f,
                Colour = Color4.Black.Opacity(0.25f),
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                background = new Sprite
                {
                    FillMode = FillMode.Fill,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourInfo = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.25f), Color4.Black.Opacity(0.75f)),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding, Left = horizontal_padding, Right = vertical_padding },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                title = new OsuSpriteText
                                {
                                    TextSize = 18,
                                    Font = @"Exo2.0-BoldItalic",
                                },
                                artist = new OsuSpriteText
                                {
                                    Font = @"Exo2.0-BoldItalic",
                                },
                                difficultyIcons = new FillFlowContainer
                                {
                                    Margin = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding },
                                    AutoSizeAxes = Axes.Both,
                                },
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Margin = new MarginPadding { Right = (height - vertical_padding * 2) + vertical_padding },
                            Children = new Drawable[]
                            {
                                playCount = new Statistic(FontAwesome.fa_play_circle, 4579492)
                                {
                                	Margin = new MarginPadding { Right = 1 },
                                },
                                favouriteCount = new Statistic(FontAwesome.fa_heart, 2659),
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
                                            Text = @"mapped by ",
                                            TextSize = 14,
                                        },
                                        mapper = new OsuSpriteText
                                        {
                                            TextSize = 14,
                                            Font = @"Exo2.0-SemiBoldItalic",
                                        },
                                    },
                                },
                                source = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    TextSize = 14,
                                },
                            },
                        },
                        new DownloadButton
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Size = new Vector2(height - (vertical_padding * 2)),
                        },
                    },
                },
            };
        }

        private class DownloadButton : ClickableContainer
        {
            //todo: proper download button animations
            public DownloadButton()
            {
                Children = new Drawable[]
                {
                    new TextAwesome
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        UseFullGlyphHeight = false,
                        TextSize = 30,
                        Icon = FontAwesome.fa_osu_chevron_down_o,
                    },
                };
            }
        }
    }
}
