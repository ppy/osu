﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Info : Container
    {
        private const float transition_duration = 250;
        private const float metadata_width = 175;
        private const float spacing = 20;
        private const float base_height = 220;

        private readonly Box successRateBackground;
        private readonly Box background;
        private readonly SuccessRate successRate;

        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();

        public BeatmapInfo Beatmap
        {
            get => successRate.Beatmap;
            set => successRate.Beatmap = value;
        }

        public Info()
        {
            MetadataSection source, tags, genre, language;
            OsuSpriteText unrankedPlaceholder;

            RelativeSizeAxes = Axes.X;
            Height = base_height;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
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
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new MetadataSection("Description"),
                            },
                        },
                        new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = metadata_width,
                            Padding = new MarginPadding { Horizontal = 10 },
                            Margin = new MarginPadding { Right = BeatmapSetOverlay.RIGHT_WIDTH + spacing },
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                                Children = new[]
                                {
                                    source = new MetadataSection("Source"),
                                    genre = new MetadataSection("Genre") { Width = 0.5f },
                                    language = new MetadataSection("Language") { Width = 0.5f },
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
                                unrankedPlaceholder = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Alpha = 0,
                                    Text = "Unranked beatmap",
                                    Font = OsuFont.GetFont(size: 12)
                                },
                            },
                        },
                    },
                },
            };

            BeatmapSet.ValueChanged += b =>
            {
                source.Text = b.NewValue?.Metadata.Source ?? string.Empty;
                tags.Text = b.NewValue?.Metadata.Tags ?? string.Empty;
                genre.Text = b.NewValue?.OnlineInfo?.Genre?.Name ?? string.Empty;
                language.Text = b.NewValue?.OnlineInfo?.Language?.Name ?? string.Empty;
                var setHasLeaderboard = b.NewValue?.OnlineInfo?.Status > 0;
                successRate.Alpha = setHasLeaderboard ? 1 : 0;
                unrankedPlaceholder.Alpha = setHasLeaderboard ? 0 : 1;
                Height = setHasLeaderboard ? 270 : base_height;
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            successRateBackground.Colour = colourProvider.Background4;
            background.Colour = colourProvider.Background5;
        }

        private class MetadataSection : FillFlowContainer
        {
            private readonly TextFlowContainer textFlow;

            public string Text
            {
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Hide();
                        return;
                    }

                    this.FadeIn(transition_duration);
                    textFlow.Clear();
                    textFlow.AddText(value, s => s.Font = s.Font.With(size: 12));
                }
            }

            public MetadataSection(string title)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Spacing = new Vector2(5f);

                InternalChildren = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = title,
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                        Margin = new MarginPadding { Top = 15 },
                    },
                    textFlow = new OsuTextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                };
            }
        }
    }
}
