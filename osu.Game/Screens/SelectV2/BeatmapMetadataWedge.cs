// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMetadataWedge : VisibilityContainer
    {
        private MetadataDisplay creator = null!;
        private MetadataDisplay source = null!;
        private MetadataDisplay genre = null!;
        private MetadataDisplay language = null!;
        private MetadataDisplay userTags = null!;
        private MetadataDisplay mapperTags = null!;
        private MetadataDisplay submitted = null!;
        private MetadataDisplay ranked = null!;

        private Drawable ratingsWedge = null!;
        private SuccessRateDisplay successRateDisplay = null!;
        private UserRatingDisplay userRatingDisplay = null!;
        private RatingSpreadDisplay ratingSpreadDisplay = null!;

        private Drawable failRetryWedge = null!;
        private FailRetryDisplay failRetryDisplay = null!;

        public bool RatingsVisible => ratingsWedge.Alpha > 0;
        public bool FailRetryVisible => failRetryWedge.Alpha > 0;

        protected override bool StartHidden => true;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IBindable<APIState> apiState = null!;

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        [Resolved]
        private SongSelect? songSelect { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Top = 4f };

            Width = 0.9f;

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
                                        AutoSizeDuration = (float)transition_duration / 3,
                                        AutoSizeEasing = Easing.OutQuint,
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
                                                                creator = new MetadataDisplay("Creator"),
                                                                genre = new MetadataDisplay("Genre"),
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
                                                                source = new MetadataDisplay("Source"),
                                                                language = new MetadataDisplay("Language"),
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
                                                                submitted = new MetadataDisplay("Submitted"),
                                                                ranked = new MetadataDisplay("Ranked"),
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                            userTags = new MetadataDisplay("User Tags")
                                            {
                                                Alpha = 0,
                                            },
                                            mapperTags = new MetadataDisplay("Mapper Tags"),
                                        },
                                    },
                                },
                            },
                        },
                    }),
                    new ShearAligningWrapper(ratingsWedge = new Container
                    {
                        Alpha = 0f,
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
                                        successRateDisplay = new SuccessRateDisplay(),
                                        Empty(),
                                        userRatingDisplay = new UserRatingDisplay(),
                                        Empty(),
                                        ratingSpreadDisplay = new RatingSpreadDisplay(),
                                    },
                                },
                            },
                        }
                    }),
                    new ShearAligningWrapper(failRetryWedge = new Container
                    {
                        Alpha = 0f,
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
                                Child = failRetryDisplay = new FailRetryDisplay(),
                            },
                        },
                    }),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmap.BindValueChanged(_ => updateDisplay());

            apiState = api.State.GetBoundCopy();
            apiState.BindValueChanged(_ => Scheduler.AddOnce(updateDisplay), true);
        }

        private const double transition_duration = 300;

        protected override void PopIn()
        {
            this.FadeIn(transition_duration, Easing.OutQuint)
                .MoveToX(0, transition_duration, Easing.OutQuint);

            updateSubWedgeVisibility();
        }

        protected override void PopOut()
        {
            this.FadeOut(transition_duration, Easing.OutQuint)
                .MoveToX(-100, transition_duration, Easing.OutQuint);

            updateSubWedgeVisibility();
        }

        private void updateSubWedgeVisibility()
        {
            // We could consider hiding individual wedges based on zero data in the future.
            // Needs some experimentation on what looks good.

            var beatmapInfo = beatmap.Value.BeatmapInfo;
            var currentOnlineBeatmap = currentOnlineBeatmapSet?.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmapInfo.OnlineID);

            if (State.Value == Visibility.Visible && currentOnlineBeatmap != null)
            {
                ratingsWedge.FadeIn(transition_duration, Easing.OutQuint)
                            .MoveToX(0, transition_duration, Easing.OutQuint);

                failRetryWedge.Delay(100)
                              .FadeIn(transition_duration, Easing.OutQuint)
                              .MoveToX(0, transition_duration, Easing.OutQuint);
            }
            else
            {
                failRetryWedge.FadeOut(transition_duration, Easing.OutQuint)
                              .MoveToX(-50, transition_duration, Easing.OutQuint);

                ratingsWedge.Delay(100)
                            .FadeOut(transition_duration, Easing.OutQuint)
                            .MoveToX(-50, transition_duration, Easing.OutQuint);
            }
        }

        private void updateDisplay()
        {
            var metadata = beatmap.Value.Metadata;
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            creator.Data = (metadata.Author.Username, () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, metadata.Author)));

            if (!string.IsNullOrEmpty(metadata.Source))
                source.Data = (metadata.Source, () => songSelect?.Search(metadata.Source));
            else
                source.Data = ("-", null);

            mapperTags.Tags = (metadata.Tags.Split(' '), t => songSelect?.Search(t));
            submitted.Date = beatmapSetInfo.DateSubmitted;
            ranked.Date = beatmapSetInfo.DateRanked;

            if (currentOnlineBeatmapSet == null || currentOnlineBeatmapSet.OnlineID != beatmapSetInfo.OnlineID)
                refetchBeatmapSet();

            updateOnlineDisplay();
        }

        private APIBeatmapSet? currentOnlineBeatmapSet;
        private GetBeatmapSetRequest? currentRequest;

        private void refetchBeatmapSet()
        {
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            currentRequest?.Cancel();
            currentRequest = null;
            currentOnlineBeatmapSet = null;

            if (beatmapSetInfo.OnlineID >= 1)
            {
                // todo: consider introducing a BeatmapSetLookupCache for caching benefits.
                currentRequest = new GetBeatmapSetRequest(beatmapSetInfo.OnlineID);
                currentRequest.Failure += _ => updateOnlineDisplay();
                currentRequest.Success += s =>
                {
                    currentOnlineBeatmapSet = s;
                    updateOnlineDisplay();
                };

                api.Queue(currentRequest);
            }
        }

        private void updateOnlineDisplay()
        {
            if (currentRequest?.CompletionState == APIRequestCompletionState.Waiting)
            {
                genre.Data = null;
                language.Data = null;
                userTags.Tags = null;
                return;
            }

            if (currentOnlineBeatmapSet == null)
            {
                genre.Data = ("-", null);
                language.Data = ("-", null);
            }
            else
            {
                var beatmapInfo = beatmap.Value.BeatmapInfo;

                var onlineBeatmapSet = currentOnlineBeatmapSet;
                var onlineBeatmap = onlineBeatmapSet.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmapInfo.OnlineID);

                genre.Data = (onlineBeatmapSet.Genre.Name, () => songSelect?.Search(onlineBeatmapSet.Genre.Name));
                language.Data = (onlineBeatmapSet.Language.Name, () => songSelect?.Search(onlineBeatmapSet.Language.Name));

                if (onlineBeatmap != null)
                {
                    userRatingDisplay.Data = onlineBeatmapSet.Ratings;
                    ratingSpreadDisplay.Data = onlineBeatmapSet.Ratings;
                    successRateDisplay.Data = (onlineBeatmap.PassCount, onlineBeatmap.PlayCount);
                    failRetryDisplay.Data = onlineBeatmap.FailTimes ?? new APIFailTimes();
                }
            }

            updateUserTags();
            updateSubWedgeVisibility();
        }

        private void updateUserTags()
        {
            var beatmapInfo = beatmap.Value.BeatmapInfo;
            var onlineBeatmapSet = currentOnlineBeatmapSet;
            var onlineBeatmap = onlineBeatmapSet?.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmapInfo.OnlineID);

            if (onlineBeatmap?.TopTags == null || onlineBeatmap.TopTags.Length == 0 || onlineBeatmapSet?.RelatedTags == null)
            {
                userTags.FadeOut(transition_duration, Easing.OutQuint);
                return;
            }

            var tagsById = onlineBeatmapSet.RelatedTags.ToDictionary(t => t.Id);
            string[] userTagsArray = onlineBeatmap.TopTags
                                                  .Select(t => (topTag: t, relatedTag: tagsById.GetValueOrDefault(t.TagId)))
                                                  .Where(t => t.relatedTag != null)
                                                  // see https://github.com/ppy/osu-web/blob/bb3bd2e7c6f84f26066df5ea20a81c77ec9bb60a/resources/js/beatmapsets-show/controller.ts#L103-L106 for sort criteria
                                                  .OrderByDescending(t => t.topTag.VoteCount)
                                                  .ThenBy(t => t.relatedTag!.Name)
                                                  .Select(t => t.relatedTag!.Name)
                                                  .ToArray();

            userTags.FadeIn(transition_duration, Easing.OutQuint);
            userTags.Tags = (userTagsArray, t => songSelect?.Search(t));
        }
    }
}
