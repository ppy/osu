// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Header : Container
    {
        private const float transition_duration = 250;
        private const float tabs_height = 50;
        private const float buttons_height = 45;
        private const float buttons_spacing = 5;

        private readonly Box tabsBg;
        private readonly Container coverContainer;
        private readonly OsuSpriteText title, artist;
        private readonly Container noVideoButtons;
        private readonly FillFlowContainer videoButtons;
        private readonly AuthorInfo author;
        public Details Details;

        private DelayedLoadWrapper cover;

        public readonly BeatmapPicker Picker;

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                Picker.BeatmapSet = author.BeatmapSet = Details.BeatmapSet = BeatmapSet;
                title.Text = BeatmapSet.Metadata.Title;
                artist.Text = BeatmapSet.Metadata.Artist;

                noVideoButtons.FadeTo(BeatmapSet.OnlineInfo.HasVideo ? 0 : 1, transition_duration);
                videoButtons.FadeTo(BeatmapSet.OnlineInfo.HasVideo ? 1 : 0, transition_duration);

                cover?.FadeOut(400, Easing.Out);
                coverContainer.Add(cover = new DelayedLoadWrapper(new BeatmapSetCover(BeatmapSet)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    OnLoadComplete = d =>
                    {
                        d.FadeInFromZero(400, Easing.Out);
                    },
                })
                {
                    RelativeSizeAxes = Axes.Both,
                    TimeBeforeLoad = 300
                });
            }
        }

        public Header()
        {
            RelativeSizeAxes = Axes.X;
            Height = 400;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.25f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = tabs_height,
                    Children = new[]
                    {
                        tabsBg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = tabs_height },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                },
                                coverContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.3f), Color4.Black.Opacity(0.8f)),
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 20, Bottom = 30, Horizontal = BeatmapSetOverlay.X_PADDING },
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 113,
                                        Child = Picker = new BeatmapPicker(),
                                    },
                                    title = new OsuSpriteText
                                    {
                                        Font = @"Exo2.0-BoldItalic",
                                        TextSize = 37,
                                    },
                                    artist = new OsuSpriteText
                                    {
                                        Font = @"Exo2.0-SemiBoldItalic",
                                        TextSize = 25,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Top = 20 },
                                        Child = author = new AuthorInfo(),
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = buttons_height,
                                        Margin = new MarginPadding { Top = 10 },
                                        Children = new Drawable[]
                                        {
                                            new FavouriteButton(),
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Left = buttons_height + buttons_spacing },
                                                Children = new Drawable[]
                                                {
                                                    noVideoButtons = new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Alpha = 0f,
                                                        Child = new DownloadButton("Download", @""),
                                                    },
                                                    videoButtons = new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(buttons_spacing),
                                                        Alpha = 0f,
                                                        Children = new[]
                                                        {
                                                            new DownloadButton("Download", "with Video"),
                                                            new DownloadButton("Download", "without Video"),
                                                        },
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                        Details = new Details
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Margin = new MarginPadding { Right = BeatmapSetOverlay.X_PADDING },
                        },
                    },
                },
            };

            Picker.Beatmap.ValueChanged += b => Details.Beatmap = b;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabsBg.Colour = colours.Gray3;
        }
    }
}
