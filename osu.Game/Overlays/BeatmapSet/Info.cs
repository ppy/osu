// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.BeatmapSet
{
    public class Info : Container
    {
        private const float metadata_width = 175;
        private const float spacing = 20;
        private const float base_height = 220;

        private readonly Box successRateBackground;
        private readonly Box background;
        private readonly SuccessRate successRate;

        public readonly Bindable<APIBeatmapSet> BeatmapSet = new Bindable<APIBeatmapSet>();

        public APIBeatmap BeatmapInfo
        {
            get => successRate.Beatmap;
            set => successRate.Beatmap = value;
        }

        public Info()
        {
            MetadataSection source, tags, genre, language;
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
                                Child = new MetadataSection(MetadataType.Description),
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
                                    source = new MetadataSection(MetadataType.Source),
                                    genre = new MetadataSection(MetadataType.Genre) { Width = 0.5f },
                                    language = new MetadataSection(MetadataType.Language) { Width = 0.5f },
                                    tags = new MetadataSection(MetadataType.Tags),
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
                source.Text = b.NewValue?.Source ?? string.Empty;
                tags.Text = b.NewValue?.Tags ?? string.Empty;
                genre.Text = b.NewValue?.Genre.Name ?? string.Empty;
                language.Text = b.NewValue?.Language.Name ?? string.Empty;
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
