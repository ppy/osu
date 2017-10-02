﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Info : Container
    {
        private const float transition_duration = 250;
        private const float metadata_width = 225;
        private const float spacing = 20;

        private readonly MetadataSection description, source, tags;
        private readonly Box successRateBackground;
        private readonly SuccessRate successRate;

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                source.Text = BeatmapSet.Metadata.Source;
                tags.Text = BeatmapSet.Metadata.Tags;
            }
        }

        public BeatmapInfo Beatmap
        {
            get { return successRate.Beatmap; }
            set { successRate.Beatmap = value; }
        }

        public Info()
        {
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
                    Padding = new MarginPadding { Top = 15, Horizontal = BeatmapSetOverlay.X_PADDING },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = metadata_width + BeatmapSetOverlay.RIGHT_WIDTH + spacing * 2 },
                            Child = new ScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                Child = description = new MetadataSection("Description"),
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
                            Margin = new MarginPadding { Right = BeatmapSetOverlay.RIGHT_WIDTH + spacing },
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                LayoutDuration = transition_duration,
                                Children = new[]
                                {
                                    source = new MetadataSection("Source"),
                                    tags = new MetadataSection("Tags"),
                                },
                            },
                        },
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = BeatmapSetOverlay.RIGHT_WIDTH,
                            Children = new Drawable[]
                            {
                                successRateBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                successRate = new SuccessRate
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Top = 20, Horizontal = 15 },
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
            source.TextColour = description.TextColour = colours.Gray5;
            tags.TextColour = colours.BlueDark;
        }

        private class MetadataSection : FillFlowContainer
        {
            private readonly OsuSpriteText header;
            private readonly TextFlowContainer textFlow;

            public string Text
            {
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.FadeOut(transition_duration);
                        return;
                    }

                    this.FadeIn(transition_duration);
                    textFlow.Clear();
                    textFlow.AddText(value, s => s.TextSize = 14);
                }
            }

            public Color4 TextColour
            {
                get { return textFlow.Colour; }
                set { textFlow.Colour = value; }
            }

            public MetadataSection(string title)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Spacing = new Vector2(5f);

                InternalChildren = new Drawable[]
                {
                    header = new OsuSpriteText
                    {
                        Text = title,
                        Font = @"Exo2.0-Bold",
                        TextSize = 14,
                        Shadow = false,
                        Margin = new MarginPadding { Top = 20 },
                    },
                    textFlow = new TextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                header.Colour = colours.Gray5;
            }
        }
    }
}
