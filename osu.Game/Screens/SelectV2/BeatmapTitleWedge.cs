// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapTitleWedge : VisibilityContainer
    {
        private const float corner_radius = 10;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private IBindable<SongSelect.BeatmapSetLookupResult?> onlineLookupResult { get; set; } = null!;

        protected override bool StartHidden => true;

        private ModSettingChangeTracker? settingChangeTracker;

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private OsuHoverContainer titleLink = null!;
        private MarqueeContainer titleLabel = null!;
        private OsuHoverContainer artistLink = null!;
        private MarqueeContainer artistLabel = null!;

        internal string DisplayedTitle { get; private set; } = string.Empty;
        internal string DisplayedArtist { get; private set; } = string.Empty;

        private StatisticPlayCount playCount = null!;
        private FavouriteButton favouriteButton = null!;
        private Statistic lengthStatistic = null!;
        private Statistic bpmStatistic = null!;

        [Resolved]
        private ISongSelect? songSelect { get; set; }

        [Resolved]
        private LocalisationManager localisation { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private FillFlowContainer statisticsFlow = null!;

        public BeatmapTitleWedge()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = corner_radius;

            InternalChildren = new Drawable[]
            {
                new WedgeBackground(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding
                    {
                        Top = SongSelect.WEDGE_CONTENT_MARGIN,
                        Left = SongSelect.WEDGE_CONTENT_MARGIN
                    },
                    Spacing = new Vector2(0f, 4f),
                    Children = new Drawable[]
                    {
                        new ShearAligningWrapper(statusPill = new BeatmapSetOnlineStatusPill
                        {
                            Shear = -OsuGame.SHEAR,
                            ShowUnknownStatus = true,
                            TextSize = OsuFont.Style.Caption1.Size,
                            TextPadding = new MarginPadding { Horizontal = 6, Vertical = 1 },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Title.Size,
                            Margin = new MarginPadding { Bottom = -4f },
                            Child = titleLink = new OsuHoverContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Child = titleLabel = new MarqueeContainer
                                {
                                    OverflowSpacing = 50,
                                }
                            }
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Heading2.Size,
                            Margin = new MarginPadding { Left = 1f },
                            Child = artistLink = new OsuHoverContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Child = artistLabel = new MarqueeContainer
                                {
                                    OverflowSpacing = 50,
                                }
                            }
                        }),
                        new ShearAligningWrapper(statisticsFlow = new FillFlowContainer
                        {
                            Shear = -OsuGame.SHEAR,
                            AutoSizeAxes = Axes.X,
                            Height = 30,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2f, 0f),
                            Children = new Drawable[]
                            {
                                playCount = new StatisticPlayCount(background: true, leftPadding: SongSelect.WEDGE_CONTENT_MARGIN, minSize: 50f)
                                {
                                    Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                                },
                                favouriteButton = new FavouriteButton(),
                                lengthStatistic = new Statistic(OsuIcon.Clock),
                                bpmStatistic = new Statistic(OsuIcon.Metronome)
                                {
                                    TooltipText = BeatmapsetsStrings.ShowStatsBpm,
                                    Margin = new MarginPadding { Left = 5f },
                                },
                            },
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Padding = new MarginPadding { Right = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Child = new DifficultyDisplay(),
                        }),
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            working.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());
            onlineLookupResult.BindValueChanged(_ => updateDisplay());

            mods.BindValueChanged(m =>
            {
                settingChangeTracker?.Dispose();

                updateLengthAndBpmStatistics();

                settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                settingChangeTracker.SettingChanged += _ => updateLengthAndBpmStatistics();
            });

            updateDisplay();

            statisticsFlow.AutoSizeDuration = 100;
            statisticsFlow.AutoSizeEasing = Easing.OutQuint;
        }

        protected override void PopIn()
        {
            this.MoveToX(0, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeIn(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        protected override void PopOut()
        {
            this.MoveToX(-150, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeOut(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        private void updateDisplay()
        {
            var metadata = working.Value.Metadata;
            var beatmapInfo = working.Value.BeatmapInfo;

            statusPill.Status = beatmapInfo.Status;

            var titleText = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            titleLabel.CreateContent = () => new OsuSpriteText
            {
                Text = titleText,
                Shadow = true,
                Font = OsuFont.Style.Title,
            };
            titleLink.Action = () => songSelect?.Search(titleText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));
            DisplayedTitle = titleText.ToString();

            var artistText = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);
            artistLabel.CreateContent = () => new OsuSpriteText
            {
                Text = artistText,
                Shadow = true,
                Font = OsuFont.Style.Heading2,
            };
            artistLink.Action = () => songSelect?.Search(artistText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));
            DisplayedArtist = artistText.ToString();

            updateLengthAndBpmStatistics();
            updateOnlineDisplay();
        }

        private CancellationTokenSource? lengthBpmCancellationSource;

        private void updateLengthAndBpmStatistics()
        {
            lengthBpmCancellationSource?.Cancel();
            lengthBpmCancellationSource = new CancellationTokenSource();

            var token = lengthBpmCancellationSource.Token;

            Task.Run(() =>
            {
                var beatmapInfo = working.Value.BeatmapInfo;
                // This can take time as it is a synchronous task.
                var beatmap = working.Value.Beatmap;

                double rate = ModUtils.CalculateRateWithMods(mods.Value);

                int bpmMax = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMaximum, rate);
                int bpmMin = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMinimum, rate);
                int mostCommonBPM = FormatUtils.RoundBPM(60000 / beatmap.GetMostCommonBeatLength(), rate);

                double drainLength = Math.Round(beatmap.CalculateDrainLength() / rate);
                double hitLength = Math.Round(beatmapInfo.Length / rate);

                Schedule(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    lengthStatistic.Text = hitLength.ToFormattedDuration();
                    lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

                    bpmStatistic.Text = bpmMin == bpmMax
                        ? $"{bpmMin}"
                        : $"{bpmMin}-{bpmMax} (mostly {mostCommonBPM})";
                });
            }, token);
        }

        private CancellationTokenSource? onlineDisplayCancellationSource;

        private void updateOnlineDisplay()
        {
            onlineDisplayCancellationSource?.Cancel();
            onlineDisplayCancellationSource = null;

            if (onlineLookupResult.Value?.Status != SongSelect.BeatmapSetLookupStatus.Completed)
            {
                playCount.Value = null;
                favouriteButton.SetLoading();
            }
            else
            {
                var onlineBeatmap = onlineLookupResult.Value.Result?.Beatmaps.SingleOrDefault(b => b.OnlineID == working.Value.BeatmapInfo.OnlineID);
                playCount.Value = new StatisticPlayCount.Data(onlineBeatmap?.PlayCount ?? -1, onlineBeatmap?.UserPlayCount ?? -1);
                favouriteButton.SetBeatmapSet(onlineLookupResult.Value.Result);

                onlineDisplayCancellationSource = new CancellationTokenSource();
                var token = onlineDisplayCancellationSource.Token;

                // the online fetch may have also updated the beatmap's status.
                // this needs to be checked against the *local* beatmap model rather than the online one, because it's not known here whether the status change has occurred or not
                // (think scenarios like the beatmap being locally modified).
                // it also has to be handled explicitly like this because the working beatmap's `BeatmapInfo` will not receive these updates due to being detached
                // (and because of https://github.com/ppy/osu/blob/4b73afd1957a9161e2956fc4191c8114d9958372/osu.Game/Screens/SelectV2/SongSelect.cs#L487-L488
                // which prevents working beatmap refetches caused by changes to the realm model of perceived low importance).
                realm.RunAsync(r =>
                {
                    var refetchedBeatmap = r.Find<BeatmapInfo>(working.Value.BeatmapInfo.ID);
                    return refetchedBeatmap?.Status;
                }, token).ContinueWith(t =>
                {
                    var status = t.GetResultSafely();

                    if (status != null)
                    {
                        Schedule(() =>
                        {
                            if (token.IsCancellationRequested)
                                return;

                            statusPill.Status = status.Value;
                        });
                    }
                }, token);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            onlineDisplayCancellationSource?.Dispose();
            onlineDisplayCancellationSource = null;
            base.Dispose(isDisposing);
        }
    }
}
