// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class Info : Container
    {
        private const float metadata_width = 185;
        private const float spacing = 20;
        private const float base_height = 300;

        private readonly Box successRateBackground;
        private readonly Box background;
        private readonly MetadataSection<string[]?> userTags;

        public readonly Bindable<APIBeatmapSet> BeatmapSet = new Bindable<APIBeatmapSet>();
        public readonly Bindable<APIBeatmap> Beatmap = new Bindable<APIBeatmap>();

        public Info()
        {
            SuccessRate successRate;
            MetadataSectionNominators nominators;
            MetadataSection source, mapperTags;
            MetadataSectionGenre genre;
            MetadataSectionLanguage language;

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
                        new OsuScrollContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = metadata_width,
                            Padding = new MarginPadding { Left = 10 },
                            Margin = new MarginPadding { Right = BeatmapSetOverlay.RIGHT_WIDTH + spacing },
                            Masking = true,
                            ScrollbarOverlapsContent = false,
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Full,
                                Padding = new MarginPadding { Right = 5 },
                                Children = new Drawable[]
                                {
                                    nominators = new MetadataSectionNominators(),
                                    source = new MetadataSectionSource(),
                                    genre = new MetadataSectionGenre { Width = 0.5f },
                                    language = new MetadataSectionLanguage { Width = 0.5f },
                                    userTags = new MetadataSectionUserTags(),
                                    mapperTags = new MetadataSectionMapperTags(),
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

            BeatmapSet.BindValueChanged(b =>
            {
                nominators.Metadata = (b.NewValue?.CurrentNominations ?? Array.Empty<BeatmapSetOnlineNomination>(), b.NewValue?.RelatedUsers ?? Array.Empty<APIUser>());
                source.Metadata = b.NewValue?.Source ?? string.Empty;
                mapperTags.Metadata = b.NewValue?.Tags ?? string.Empty;
                updateUserTags();
                genre.Metadata = b.NewValue?.Genre ?? new BeatmapSetOnlineGenre { Id = (int)SearchGenre.Unspecified };
                language.Metadata = b.NewValue?.Language ?? new BeatmapSetOnlineLanguage { Id = (int)SearchLanguage.Unspecified };
            });
            Beatmap.BindValueChanged(b =>
            {
                successRate.Beatmap = b.NewValue;
                updateUserTags();
            });
        }

        private void updateUserTags()
        {
            if (Beatmap.Value?.TopTags == null || Beatmap.Value.TopTags.Length == 0 || BeatmapSet.Value?.RelatedTags == null)
            {
                userTags.Metadata = null;
                return;
            }

            var tagsById = BeatmapSet.Value.RelatedTags.ToDictionary(t => t.Id);
            userTags.Metadata = Beatmap.Value.TopTags
                                       .Select(t => (topTag: t, relatedTag: tagsById.GetValueOrDefault(t.TagId)))
                                       .Where(t => t.relatedTag != null)
                                       // see https://github.com/ppy/osu-web/blob/bb3bd2e7c6f84f26066df5ea20a81c77ec9bb60a/resources/js/beatmapsets-show/controller.ts#L103-L106 for sort criteria
                                       .OrderByDescending(t => t.topTag.VoteCount)
                                       .ThenBy(t => t.relatedTag!.Name)
                                       .Select(t => t.relatedTag!.Name)
                                       .ToArray();
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            successRateBackground.Colour = colourProvider.Background4;
            background.Colour = colourProvider.Background5;
        }
    }
}
