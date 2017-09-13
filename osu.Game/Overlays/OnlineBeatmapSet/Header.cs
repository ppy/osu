// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
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

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class Header : Container
    {
        private const float transition_duration = 250;
        private const float tabs_height = 50;

        private readonly Box tabsBg;

        public readonly BeatmapPicker Picker;

        public Header(BeatmapSetInfo set)
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

            DownloadButton noVideo, withVideo, withoutVideo;
            Details details;
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
                                new DelayedLoadWrapper(new BeatmapSetCover(set)
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
                            Padding = new MarginPadding { Top = 20, Bottom = 30, Horizontal = OnlineBeatmapSetOverlay.X_PADDING },
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
                                        Child = Picker = new BeatmapPicker(set),
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = set.Metadata.Title,
                                        Font = @"Exo2.0-BoldItalic",
                                        TextSize = 37,
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = set.Metadata.Artist,
                                        Font = @"Exo2.0-SemiBoldItalic",
                                        TextSize = 25,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Top = 20 },
                                        Child = new AuthorInfo(set.OnlineInfo),
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 45,
                                        Spacing = new Vector2(5f),
                                        Margin = new MarginPadding { Top = 10 },
                                        LayoutDuration = transition_duration,
                                        LayoutEasing = Easing.Out,
                                        Children = new HeaderButton[]
                                        {
                                            new FavouriteButton(),
                                            noVideo = new DownloadButton("Download", @""),
                                            withVideo = new DownloadButton("Download", "with Video"),
                                            withoutVideo = new DownloadButton("Download", "without Video"),
                                        },
                                    },
                                },
                            },
                        },
                        details = new Details(set)
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Margin = new MarginPadding { Right = OnlineBeatmapSetOverlay.X_PADDING },
                        },
                    },
                },
            };

            Picker.Beatmap.ValueChanged += b =>
            {
                details.Beatmap = b;

                if (b.OnlineInfo.HasVideo)
                {
                    noVideo.FadeOut(transition_duration);
                    withVideo.FadeIn(transition_duration);
                    withoutVideo.FadeIn(transition_duration);
                }
                else
                {
                    noVideo.FadeIn(transition_duration);
                    withVideo.FadeOut(transition_duration);
                    withoutVideo.FadeOut(transition_duration);
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabsBg.Colour = colours.Gray3;
        }
    }
}
