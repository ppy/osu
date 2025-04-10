// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapDetailsWedge : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private BeatmapDetailsWedgeStatistic creator = null!;
        private BeatmapDetailsWedgeStatistic source = null!;
        private BeatmapDetailsWedgeStatistic genre = null!;
        private BeatmapDetailsWedgeStatistic language = null!;
        private BeatmapDetailsWedgeStatistic tag = null!;
        private BeatmapDetailsWedgeStatistic submitted = null!;
        private BeatmapDetailsWedgeStatistic ranked = null!;

        private Drawable ratingsWedge = null!;
        private BeatmapDetailsSuccessRate successRate = null!;
        private BeatmapDetailsUserRating userRating = null!;
        private BeatmapDetailsRatingSpread ratingSpread = null!;

        private Drawable failRetryWedge = null!;
        private BeatmapDetailsFailRetry failRetry = null!;

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
        private void load(OverlayColourProvider colourProvider)
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
                Children = new[]
                {
                    new ShearAlignedDrawable(shear, new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Shear = shear,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background5.Opacity(0.6f),
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -shear,
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
                                                                creator = new BeatmapDetailsWedgeStatistic("Creator"),
                                                                genre = new BeatmapDetailsWedgeStatistic("Genre"),
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
                                                                source = new BeatmapDetailsWedgeStatistic("Source"),
                                                                language = new BeatmapDetailsWedgeStatistic("Language"),
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
                                                                submitted = new BeatmapDetailsWedgeStatistic("Submitted"),
                                                                ranked = new BeatmapDetailsWedgeStatistic("Ranked"),
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                            tag = new BeatmapDetailsWedgeStatistic("Tags"),
                                        },
                                    },
                                },
                            },
                        },
                    }),
                    new ShearAlignedDrawable(shear, ratingsWedge = new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Shear = shear,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background5.Opacity(0.6f),
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -shear,
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
                                        successRate = new BeatmapDetailsSuccessRate(),
                                        Empty(),
                                        userRating = new BeatmapDetailsUserRating(),
                                        Empty(),
                                        ratingSpread = new BeatmapDetailsRatingSpread(),
                                    },
                                },
                            },
                        }
                    }),
                    new ShearAlignedDrawable(shear, failRetryWedge = new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Shear = shear,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background5.Opacity(0.6f),
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -shear,
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 40f, Vertical = 16 },
                                Child = failRetry = new BeatmapDetailsFailRetry(),
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

        private void updateDisplay()
        {
            var metadata = beatmap.Value.Metadata;
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            creator.Data = (metadata.Author.Username, () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, metadata.Author)));

            if (!string.IsNullOrEmpty(metadata.Source))
                source.Data = (metadata.Source, () => songSelect?.Search(metadata.Source));
            else
                source.Data = ("-", null);

            tag.Tags = (metadata.Tags.Split(' '), t => songSelect?.Search(t));
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
            }
            else if (currentOnlineBeatmapSet == null)
            {
                genre.Data = ("-", null);
                language.Data = ("-", null);

                ratingsWedge.FadeOut(300, Easing.OutQuint);
                ratingsWedge.MoveToX(-50, 300, Easing.OutQuint);
                failRetryWedge.FadeOut(300, Easing.OutQuint);
                failRetryWedge.MoveToX(-50, 300, Easing.OutQuint);
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
                    ratingsWedge.FadeIn(300, Easing.OutQuint);
                    ratingsWedge.MoveToX(0, 300, Easing.OutQuint);
                    failRetryWedge.FadeIn(300, Easing.OutQuint);
                    failRetryWedge.MoveToX(0, 300, Easing.OutQuint);

                    userRating.Data = onlineBeatmapSet.Ratings;
                    ratingSpread.Data = onlineBeatmapSet.Ratings;
                    successRate.Data = (onlineBeatmap.PassCount, onlineBeatmap.PlayCount);
                    failRetry.Data = onlineBeatmap.FailTimes ?? new APIFailTimes();
                }
                else
                {
                    ratingsWedge.FadeOut(300, Easing.OutQuint);
                    ratingsWedge.MoveToX(-50, 300, Easing.OutQuint);
                    failRetryWedge.FadeOut(300, Easing.OutQuint);
                    failRetryWedge.MoveToX(-50, 300, Easing.OutQuint);
                }
            }
        }
    }
}
