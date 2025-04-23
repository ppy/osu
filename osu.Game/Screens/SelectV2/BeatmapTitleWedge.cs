// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
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
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        protected override bool StartHidden => true;

        private ModSettingChangeTracker? settingChangeTracker;

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private Container titleContainer = null!;
        private OsuHoverContainer titleLink = null!;
        private OsuSpriteText titleLabel = null!;
        private Container artistContainer = null!;
        private OsuHoverContainer artistLink = null!;
        private OsuSpriteText artistLabel = null!;

        internal string DisplayedTitle => titleLabel.Text.ToString();
        internal string DisplayedArtist => artistLabel.Text.ToString();

        private StatisticPlayCount playCount = null!;
        private Statistic favouritesStatistic = null!;
        private Statistic lengthStatistic = null!;
        private Statistic bpmStatistic = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        [Resolved]
        private LocalisationManager localisation { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private APIBeatmapSet? currentOnlineBeatmapSet;
        private GetBeatmapSetRequest? currentRequest;

        private FillFlowContainer statisticsFlow = null!;

        public BeatmapTitleWedge()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Shear = OsuGame.SHEAR;
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
                        new ShearAligningWrapper(titleContainer = new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Title.Size,
                            Margin = new MarginPadding { Bottom = -4f },
                            Child = titleLink = new OsuHoverContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Child = titleLabel = new TruncatingSpriteText
                                {
                                    Shadow = true,
                                    Font = OsuFont.Style.Title,
                                },
                            }
                        }),
                        new ShearAligningWrapper(artistContainer = new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Heading2.Size,
                            Margin = new MarginPadding { Left = 1f },
                            Child = artistLink = new OsuHoverContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Child = artistLabel = new TruncatingSpriteText
                                {
                                    Shadow = true,
                                    Font = OsuFont.Style.Heading2,
                                },
                            }
                        }),
                        new ShearAligningWrapper(statisticsFlow = new FillFlowContainer
                        {
                            Shear = -OsuGame.SHEAR,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2f, 0f),
                            Children = new Drawable[]
                            {
                                playCount = new StatisticPlayCount(background: true, leftPadding: SongSelect.WEDGE_CONTENT_MARGIN, minSize: 50f)
                                {
                                    Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                                },
                                favouritesStatistic = new Statistic(OsuIcon.Heart, background: true, minSize: 25f)
                                {
                                    TooltipText = BeatmapsStrings.StatusFavourites,
                                },
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

            beatmap.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());

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

        protected override void Update()
        {
            base.Update();
            titleLabel.MaxWidth = titleContainer.DrawWidth - 20;
            artistLabel.MaxWidth = artistContainer.DrawWidth - 20;
        }

        private void updateDisplay()
        {
            var metadata = beatmap.Value.Metadata;
            var beatmapInfo = beatmap.Value.BeatmapInfo;
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            statusPill.Status = beatmapInfo.Status;

            var titleText = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            titleLabel.Text = titleText;
            titleLink.Action = () => songSelect?.Search(titleText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));

            var artistText = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);
            artistLabel.Text = artistText;
            artistLink.Action = () => songSelect?.Search(artistText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));

            updateLengthAndBpmStatistics();

            if (currentOnlineBeatmapSet == null || currentOnlineBeatmapSet.OnlineID != beatmapSetInfo.OnlineID)
                refetchBeatmapSet();

            updateOnlineDisplay();
        }

        private void updateLengthAndBpmStatistics()
        {
            var beatmapInfo = beatmap.Value.BeatmapInfo;

            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            int bpmMax = FormatUtils.RoundBPM(beatmap.Value.Beatmap.ControlPointInfo.BPMMaximum, rate);
            int bpmMin = FormatUtils.RoundBPM(beatmap.Value.Beatmap.ControlPointInfo.BPMMinimum, rate);
            int mostCommonBPM = FormatUtils.RoundBPM(60000 / beatmap.Value.Beatmap.GetMostCommonBeatLength(), rate);

            double drainLength = Math.Round(beatmap.Value.Beatmap.CalculateDrainLength() / rate);
            double hitLength = Math.Round(beatmapInfo.Length / rate);

            lengthStatistic.Text = hitLength.ToFormattedDuration();
            lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

            bpmStatistic.Text = bpmMin == bpmMax
                ? $"{bpmMin}"
                : $"{bpmMin}-{bpmMax} (mostly {mostCommonBPM})";
        }

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
                playCount.Value = null;
                favouritesStatistic.Text = null;
            }
            else if (currentOnlineBeatmapSet == null)
            {
                playCount.Value = new StatisticPlayCount.Data(-1, -1);
                favouritesStatistic.Text = "-";
            }
            else
            {
                var onlineBeatmapSet = currentOnlineBeatmapSet;
                var onlineBeatmap = currentOnlineBeatmapSet.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmap.Value.BeatmapInfo.OnlineID);

                if (onlineBeatmap != null)
                {
                    playCount.FadeIn(300, Easing.OutQuint);
                    playCount.Value = new StatisticPlayCount.Data(onlineBeatmap.PlayCount, onlineBeatmap.UserPlayCount);
                }
                else
                {
                    playCount.FadeOut(300, Easing.OutQuint);
                    playCount.Value = null;
                }

                favouritesStatistic.FadeIn(300, Easing.OutQuint);
                favouritesStatistic.Text = onlineBeatmapSet.FavouriteCount.ToLocalisableString(@"N0");
            }
        }
    }
}
