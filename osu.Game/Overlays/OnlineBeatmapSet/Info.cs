// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class Info : Container
    {
        private const float metadata_width = 225;
        private const float spacing = 20;

        private readonly BeatmapSetInfo set;

        private readonly Box successRateBackground;
        private readonly FillFlowContainer metadataFlow;
        private readonly ScrollContainer descriptionScroll;

        public Info(BeatmapSetInfo set)
        {
            this.set = set;

            RelativeSizeAxes = Axes.X;
            Height = 220;
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
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 15, Horizontal = OnlineBeatmapSetOverlay.X_PADDING },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = metadata_width + OnlineBeatmapSetOverlay.RIGHT_WIDTH + spacing * 2 },
                            Child = descriptionScroll = new ScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                            },
                        },
                        new ScrollContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = metadata_width,
                            ScrollbarVisible = false,
                            Padding = new MarginPadding { Horizontal = 10 },
                            Margin = new MarginPadding { Right = OnlineBeatmapSetOverlay.RIGHT_WIDTH + spacing },
                            Child = metadataFlow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                            },
                        },
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = OnlineBeatmapSetOverlay.RIGHT_WIDTH,
                            Children = new[]
                            {
                                successRateBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            },
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            successRateBackground.Colour = colours.GrayE;
            descriptionScroll.Child = new MetadataSection("Description", "", colours.Gray5);
            metadataFlow.Children = new[]
            {
                new MetadataSection("Source", set.Metadata.Source, colours.Gray5),
                new MetadataSection("Tags", set.Metadata.Tags, colours.BlueDark),
            };
        }

        private class MetadataSection : FillFlowContainer
        {
            private readonly OsuSpriteText header;

            public MetadataSection(string title, string body, Color4 textColour)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(5f);

                TextFlowContainer content;
                Children = new Drawable[]
                {
                    header = new OsuSpriteText
                    {
                        Font = @"Exo2.0-Bold",
                        TextSize = 14,
                        Text = title,
                        Shadow = false,
                        Margin = new MarginPadding { Top = 20 },
                    },
                    content = new TextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                };

                content.AddText(body, t =>
                {
                    t.TextSize = 14;
                    t.Colour = textColour;
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                header.Colour = colours.Gray5;
            }
        }
    }
}
