// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class GameplayWarmupScreen
    {
        private partial class MetadataWedge : CompositeDrawable
        {
            private readonly APIBeatmap beatmap;

            private BeatmapMetadataWedge.MetadataDisplay creator = null!;
            private BeatmapMetadataWedge.MetadataDisplay source = null!;
            private BeatmapMetadataWedge.MetadataDisplay genre = null!;
            private BeatmapMetadataWedge.MetadataDisplay language = null!;
            private BeatmapMetadataWedge.MetadataDisplay userTags = null!;
            private BeatmapMetadataWedge.MetadataDisplay mapperTags = null!;
            private BeatmapMetadataWedge.MetadataDisplay submitted = null!;
            private BeatmapMetadataWedge.MetadataDisplay ranked = null!;

            private BeatmapMetadataWedge.SuccessRateDisplay successRateDisplay = null!;
            private BeatmapMetadataWedge.UserRatingDisplay userRatingDisplay = null!;
            private BeatmapMetadataWedge.RatingSpreadDisplay ratingSpreadDisplay = null!;
            private BeatmapMetadataWedge.FailRetryDisplay failRetryDisplay = null!;

            public MetadataWedge(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Width = 0.9f;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 4f),
                    Shear = OsuGame.SHEAR,
                    Children = new[]
                    {
                        new ShearAligningWrapper(new Container
                        {
                            CornerRadius = 10,
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new WedgeBackground(),
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Shear = -OsuGame.SHEAR,
                                    Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 35, Vertical = 16 },
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0f, 10f),
                                            Children = new Drawable[]
                                            {
                                                new GridContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                                    ColumnDimensions = new[]
                                                    {
                                                        new Dimension(),
                                                        new Dimension(),
                                                        new Dimension(),
                                                    },
                                                    Content = new[]
                                                    {
                                                        new[]
                                                        {
                                                            new FillFlowContainer
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Direction = FillDirection.Vertical,
                                                                Spacing = new Vector2(0f, 10f),
                                                                Children = new[]
                                                                {
                                                                    creator = new BeatmapMetadataWedge.MetadataDisplay(EditorSetupStrings.Creator),
                                                                    genre = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoGenre),
                                                                },
                                                            },
                                                            new FillFlowContainer
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Direction = FillDirection.Vertical,
                                                                Spacing = new Vector2(0f, 10f),
                                                                Children = new[]
                                                                {
                                                                    source = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoSource),
                                                                    language = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoLanguage),
                                                                },
                                                            },
                                                            new FillFlowContainer
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Direction = FillDirection.Vertical,
                                                                Spacing = new Vector2(0f, 10f),
                                                                Children = new[]
                                                                {
                                                                    submitted = new BeatmapMetadataWedge.MetadataDisplay(SongSelectStrings.Submitted),
                                                                    ranked = new BeatmapMetadataWedge.MetadataDisplay(SongSelectStrings.Ranked),
                                                                },
                                                            },
                                                        },
                                                    },
                                                },
                                                userTags = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoUserTags)
                                                {
                                                    Alpha = 0,
                                                },
                                                mapperTags = new BeatmapMetadataWedge.MetadataDisplay(BeatmapsetsStrings.ShowInfoMapperTags),
                                            },
                                        },
                                    },
                                },
                            },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            CornerRadius = 10,
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new WedgeBackground(),
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Shear = -OsuGame.SHEAR,
                                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(),
                                        new Dimension(GridSizeMode.Absolute, 10),
                                        new Dimension(),
                                        new Dimension(GridSizeMode.Absolute, 10),
                                        new Dimension(),
                                    },
                                    Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 40f, Vertical = 16 },
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            successRateDisplay = new BeatmapMetadataWedge.SuccessRateDisplay(),
                                            Empty(),
                                            userRatingDisplay = new BeatmapMetadataWedge.UserRatingDisplay(),
                                            Empty(),
                                            ratingSpreadDisplay = new BeatmapMetadataWedge.RatingSpreadDisplay(),
                                        },
                                    },
                                },
                            }
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            CornerRadius = 10,
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new WedgeBackground(),
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Shear = -OsuGame.SHEAR,
                                    Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 40f, Vertical = 16 },
                                    Child = failRetryDisplay = new BeatmapMetadataWedge.FailRetryDisplay(),
                                },
                            },
                        }),
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                var metadata = beatmap.Metadata;
                var beatmapSet = beatmap.BeatmapSet!;

                creator.Data = (metadata.Author.Username, null);

                if (!string.IsNullOrEmpty(metadata.Source))
                    source.Data = (metadata.Source, null);
                else
                    source.Data = ("-", null);

                if (!string.IsNullOrEmpty(metadata.Tags))
                    mapperTags.Tags = (metadata.Tags.Split(' '), _ => { });
                else
                    mapperTags.Tags = (Array.Empty<string>(), _ => { });

                submitted.Date = beatmapSet.Submitted;
                ranked.Date = beatmapSet.Ranked;

                genre.Data = (beatmapSet.Genre.Name, null);
                language.Data = (beatmapSet.Language.Name, null);

                userRatingDisplay.Data = beatmapSet.Ratings;
                ratingSpreadDisplay.Data = beatmapSet.Ratings;
                successRateDisplay.Data = (beatmap.PassCount, beatmap.PlayCount);
                failRetryDisplay.Data = beatmap.FailTimes ?? new APIFailTimes();

                var tagsById = beatmapSet.RelatedTags?.ToDictionary(t => t.Id) ?? new Dictionary<long, APITag>();
                string[] topUserTags = beatmap.TopTags?
                                              .Select(t => (topTag: t, relatedTag: tagsById.GetValueOrDefault(t.TagId)))
                                              .Where(t => t.relatedTag != null)
                                              // see https://github.com/ppy/osu-web/blob/bb3bd2e7c6f84f26066df5ea20a81c77ec9bb60a/resources/js/beatmapsets-show/controller.ts#L103-L106 for sort criteria
                                              .OrderByDescending(t => t.topTag.VoteCount)
                                              .ThenBy(t => t.relatedTag!.Name)
                                              .Select(t => t.relatedTag!.Name)
                                              .ToArray() ?? [];

                userTags.Tags = (topUserTags, _ => { });

                if (topUserTags.Length > 0)
                    userTags.Show();
            }
        }
    }
}
