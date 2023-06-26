// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class Info : Container
    {
        private const float metadata_width = 175;
        private const float spacing = 20;
        private const float base_height = 220;

        private readonly Box successRateBackground;
        private readonly Box background;
        private readonly SuccessRate successRate;

        public readonly Bindable<APIBeatmapSet> BeatmapSet = new Bindable<APIBeatmapSet>();

        public APIBeatmap? BeatmapInfo
        {
            get => successRate.Beatmap;
            set => successRate.Beatmap = value;
        }

        public Info()
        {
            MetadataSectionNominators nominators;
            MetadataSection source, tags;
            MetadataSectionGenre genre;
            MetadataSectionLanguage language;
            OsuSpriteText notRankedPlaceholder;

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
                    Padding = new MarginPadding { Top = 15, Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = metadata_width + BeatmapSetOverlay.RIGHT_WIDTH + spacing * 2 },
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new MetadataSectionDescription(),
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
                            Masking = true,
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                                Children = new Drawable[]
                                {
                                    nominators = new MetadataSectionNominators(),
                                    source = new MetadataSectionSource(),
                                    genre = new MetadataSectionGenre { Width = 0.5f },
                                    language = new MetadataSectionLanguage { Width = 0.5f },
                                    tags = new MetadataSectionTags(),
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
                                notRankedPlaceholder = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Alpha = 0,
                                    Text = "This beatmap is not ranked",
                                    Font = OsuFont.GetFont(size: 12)
                                },
                            },
                        },
                    },
                },
            };

            BeatmapSet.ValueChanged += b =>
            {
                nominators.Metadata = (b.NewValue?.CurrentNominations ?? Array.Empty<BeatmapSetOnlineNomination>(), b.NewValue?.RelatedUsers ?? Array.Empty<APIUser>());
                source.Metadata = b.NewValue?.Source ?? string.Empty;
                tags.Metadata = b.NewValue?.Tags ?? string.Empty;
                genre.Metadata = b.NewValue?.Genre ?? new BeatmapSetOnlineGenre { Id = (int)SearchGenre.Unspecified };
                language.Metadata = b.NewValue?.Language ?? new BeatmapSetOnlineLanguage { Id = (int)SearchLanguage.Unspecified };
                bool setHasLeaderboard = b.NewValue?.Status > 0;
                successRate.Alpha = setHasLeaderboard ? 1 : 0;
                notRankedPlaceholder.Alpha = setHasLeaderboard ? 0 : 1;
                Height = setHasLeaderboard ? 270 : base_height;
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            successRateBackground.Colour = colourProvider.Background4;
            background.Colour = colourProvider.Background5;
        }
    }
}
