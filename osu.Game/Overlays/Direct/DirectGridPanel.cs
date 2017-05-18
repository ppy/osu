// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Direct
{
    public class DirectGridPanel : DirectPanel
    {
        private readonly float horizontal_padding = 10;
        private readonly float vertical_padding = 5;

        private readonly Sprite background;
        private readonly OsuSpriteText title, artist, authorPrefix, author, source;
        private readonly Statistic playCount, favouriteCount;
        private readonly FillFlowContainer difficultyIcons;

        protected override Sprite Background => background;
        protected override OsuSpriteText Title => title;
        protected override OsuSpriteText Artist => artist;
        protected override OsuSpriteText Author => author;
        protected override OsuSpriteText Source => source;
        protected override Statistic PlayCount => playCount;
        protected override Statistic FavouriteCount => favouriteCount;
        protected override FillFlowContainer DifficultyIcons => difficultyIcons;

        public DirectGridPanel(BeatmapSetInfo beatmap) : base(beatmap)
        {
            Height = 140 + vertical_padding; //full height of all the elements plus vertical padding (autosize uses the image)
            CornerRadius = 4;
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
                    Colour = Color4.Black.Opacity(0.5f),
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
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
                                title = new OsuSpriteText
                                {
                                    TextSize = 18,
                                    Font = @"Exo2.0-BoldItalic",
                                },
                                artist = new OsuSpriteText
                                {
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
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding, Left = horizontal_padding, Right = horizontal_padding },
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new[]
                                            {
                                                authorPrefix = new OsuSpriteText
                                                {
                                                    Text = @"mapped by ",
                                                    TextSize = 14,
                                                    Shadow = false,
                                                },
                                                author = new OsuSpriteText
                                                {
                                                    TextSize = 14,
                                                    Font = @"Exo2.0-SemiBoldItalic",
                                                    Shadow = false,
                                                },
                                            },
                                        },
                                        source = new OsuSpriteText
                                        {
                                            TextSize = 14,
                                            Shadow = false,
                                        },
                                        difficultyIcons = new FillFlowContainer
                                        {
                                            Margin = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding  },
                                            AutoSizeAxes = Axes.Both,
                                        },
                                    },
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
                        playCount = new Statistic(FontAwesome.fa_play_circle)
                        {
	                        Margin = new MarginPadding { Right = 1 },
                        },
                        favouriteCount = new Statistic(FontAwesome.fa_heart),
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
        	authorPrefix.Colour = colours.Gray5;
        	Author.Colour = colours.BlueDark;
        	Source.Colour = colours.Gray5;
        }
    }
}
