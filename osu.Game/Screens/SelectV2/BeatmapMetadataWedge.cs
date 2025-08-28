// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Resources.Localisation.Web;
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
        private IBindable<SongSelect.BeatmapSetLookupResult> onlineLookupResult { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IBindable<APIState> apiState = null!;

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        [Resolved]
        private ISongSelect? songSelect { get; set; }

        private Sample? wedgeAppearSample;
        private Sample? wedgeHideSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
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
                                                                creator = new MetadataDisplay(EditorSetupStrings.Creator),
                                                                genre = new MetadataDisplay(BeatmapsetsStrings.ShowInfoGenre),
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
                                                                source = new MetadataDisplay(BeatmapsetsStrings.ShowInfoSource),
                                                                language = new MetadataDisplay(BeatmapsetsStrings.ShowInfoLanguage),
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
                                                                submitted = new MetadataDisplay(SongSelectStrings.Submitted),
                                                                ranked = new MetadataDisplay(SongSelectStrings.Ranked),
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                            userTags = new MetadataDisplay(BeatmapsetsStrings.ShowInfoUserTags)
                                            {
                                                Alpha = 0,
                                            },
                                            mapperTags = new MetadataDisplay(BeatmapsetsStrings.ShowInfoMapperTags),
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

            wedgeAppearSample = audio.Samples.Get(@"SongSelect/metadata-wedge-pop-in");
            wedgeHideSample = audio.Samples.Get(@"SongSelect/metadata-wedge-pop-out");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmap.BindValueChanged(_ => updateDisplay());
            onlineLookupResult.BindValueChanged(_ => updateDisplay());

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
            var currentOnlineBeatmap = onlineLookupResult.Value?.Result?.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmapInfo.OnlineID);

            if (State.Value == Visibility.Visible && currentOnlineBeatmap != null)
            {
                // play show sounds only if the wedges were previously hidden
                if (ratingsWedge.Alpha < 1)
                    playWedgeAppearSound();

                ratingsWedge.FadeIn(transition_duration, Easing.OutQuint)
                            .MoveToX(0, transition_duration, Easing.OutQuint);

                failRetryWedge.Delay(100)
                              .FadeIn(transition_duration, Easing.OutQuint)
                              .MoveToX(0, transition_duration, Easing.OutQuint);
            }
            else
            {
                // play hide sounds only if the wedges were previously visible
                if (ratingsWedge.Alpha > 0)
                    playWedgeHideSound();

                failRetryWedge.FadeOut(transition_duration, Easing.OutQuint)
                              .MoveToX(-50, transition_duration, Easing.OutQuint);

                ratingsWedge.Delay(100)
                            .FadeOut(transition_duration, Easing.OutQuint)
                            .MoveToX(-50, transition_duration, Easing.OutQuint);
            }
        }

        private void playWedgeAppearSound()
        {
            var wedgeAppearChannel1 = wedgeAppearSample?.GetChannel();
            if (wedgeAppearChannel1 == null)
                return;

            wedgeAppearChannel1.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH / 2;
            wedgeAppearChannel1.Frequency.Value = 0.98f + RNG.NextDouble(0.04f);
            wedgeAppearChannel1.Play();

            Scheduler.AddDelayed(() =>
            {
                var wedgeAppearChannel2 = wedgeAppearSample?.GetChannel();
                if (wedgeAppearChannel2 == null)
                    return;

                wedgeAppearChannel2.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH / 2;
                wedgeAppearChannel2.Frequency.Value = 0.90f + RNG.NextDouble(0.05f);
                wedgeAppearChannel2.Play();
            }, 100);
        }

        private void playWedgeHideSound()
        {
            var wedgeHideChannel = wedgeHideSample?.GetChannel();
            if (wedgeHideChannel == null)
                return;

            wedgeHideChannel.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH / 2;
            wedgeHideChannel.Play();
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

            if (!string.IsNullOrEmpty(metadata.Tags))
                mapperTags.Tags = (metadata.Tags.Split(' '), t => songSelect?.Search(t));
            else
                mapperTags.Tags = (Array.Empty<string>(), _ => { });

            submitted.Date = beatmapSetInfo.DateSubmitted;
            ranked.Date = beatmapSetInfo.DateRanked;

            updateOnlineDisplay();
        }

        private void updateOnlineDisplay()
        {
            if (onlineLookupResult.Value?.Status != SongSelect.BeatmapSetLookupStatus.Completed)
            {
                genre.Data = null;
                language.Data = null;
                userTags.Tags = null;
                return;
            }

            if (onlineLookupResult.Value.Result == null)
            {
                genre.Data = ("-", null);
                language.Data = ("-", null);
            }
            else
            {
                var beatmapInfo = beatmap.Value.BeatmapInfo;

                var onlineBeatmapSet = onlineLookupResult.Value.Result;
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
            string[] tags = realm.Run(r =>
            {
                // need to refetch because `beatmap.Value.BeatmapInfo` is not going to have the latest tags
                r.Refresh();
                var refetchedBeatmap = r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID);
                return refetchedBeatmap?.Metadata.UserTags.ToArray() ?? [];
            });

            if (tags.Length == 0)
            {
                userTags.FadeOut(transition_duration, Easing.OutQuint);
                return;
            }

            userTags.FadeIn(transition_duration, Easing.OutQuint);
            userTags.Tags = (tags, t => songSelect?.Search($@"tag=""{t}""!"));
        }
    }
}
