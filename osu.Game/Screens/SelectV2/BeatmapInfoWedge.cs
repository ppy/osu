// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapInfoWedge : VisibilityContainer
    {
        private const float corner_radius = 10;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private ModSettingChangeTracker? settingChangeTracker;

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private Container titleContainer = null!;
        private OsuHoverContainer titleLink = null!;
        private OsuSpriteText titleLabel = null!;
        private Container artistContainer = null!;
        private OsuHoverContainer artistLink = null!;
        private OsuSpriteText artistLabel = null!;

        private WedgeStatisticPlayCount playCount = null!;
        private WedgeStatistic favouritesStatistic = null!;
        private WedgeStatistic lengthStatistic = null!;
        private WedgeStatistic bpmStatistic = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        [Resolved]
        private LocalisationManager localisation { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private APIBeatmapSet? currentOnlineBeatmapSet;
        private GetBeatmapSetRequest? currentRequest;

        public BeatmapInfoWedge()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Shear = shear;
            Masking = true;
            CornerRadius = corner_radius;
            Margin = new MarginPadding { Top = -corner_radius };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3.Opacity(0.9f),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                    Spacing = new Vector2(0f, 4f),
                    Shear = -shear,
                    Children = new Drawable[]
                    {
                        new ShearAlignedDrawable(shear, new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 35,
                            Child = statusPill = new BeatmapSetOnlineStatusPill
                            {
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Right = 20f, Top = 20f },
                                TextSize = 11,
                                TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                            }
                        }),
                        new ShearAlignedDrawable(shear, titleContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 36f,
                            Margin = new MarginPadding { Bottom = -5f },
                            Child = titleLink = new OsuHoverContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Child = titleLabel = new TruncatingSpriteText
                                {
                                    Shadow = true,
                                    Font = OsuFont.TorusAlternate.With(size: 36f, weight: FontWeight.SemiBold),
                                },
                            }
                        }),
                        new ShearAlignedDrawable(shear, artistContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 24f,
                            Margin = new MarginPadding { Left = 1f },
                            Child = artistLink = new OsuHoverContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Child = artistLabel = new TruncatingSpriteText
                                {
                                    Shadow = true,
                                    Font = OsuFont.Torus.With(size: 24f, weight: FontWeight.SemiBold),
                                },
                            }
                        }),
                        new ShearAlignedDrawable(shear, new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2f, 0f),
                            AutoSizeDuration = 100,
                            AutoSizeEasing = Easing.OutQuint,
                            Children = new Drawable[]
                            {
                                playCount = new WedgeStatisticPlayCount(background: true, leftPadding: SongSelect.WEDGE_CONTENT_MARGIN, minSize: 50f)
                                {
                                    Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                                },
                                favouritesStatistic = new WedgeStatistic(OsuIcon.Heart, background: true, minSize: 25f)
                                {
                                    TooltipText = BeatmapsStrings.StatusFavourites,
                                },
                                lengthStatistic = new WedgeStatistic(OsuIcon.Clock),
                                bpmStatistic = new WedgeStatistic(OsuIcon.Metronome)
                                {
                                    TooltipText = BeatmapsetsStrings.ShowStatsBpm,
                                    Margin = new MarginPadding { Left = 5f },
                                },
                            },
                        }),
                        new ShearAlignedDrawable(shear, new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Padding = new MarginPadding { Right = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Child = new WedgeDifficultyDisplay(),
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

            FinishTransforms(true);
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

            lengthStatistic.Value = hitLength.ToFormattedDuration();
            lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

            bpmStatistic.Value = bpmMin == bpmMax
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
                favouritesStatistic.Value = null;
            }
            else if (currentOnlineBeatmapSet == null)
            {
                playCount.FadeOut(300, Easing.OutQuint);
                favouritesStatistic.FadeOut(300, Easing.OutQuint);
            }
            else
            {
                var onlineBeatmapSet = currentOnlineBeatmapSet;
                var onlineBeatmap = currentOnlineBeatmapSet.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmap.Value.BeatmapInfo.OnlineID);

                if (onlineBeatmap != null)
                {
                    playCount.FadeIn(300, Easing.OutQuint);
                    playCount.Value = new WedgeStatisticPlayCount.Data(onlineBeatmap.PlayCount, onlineBeatmap.UserPlayCount);
                }
                else
                {
                    playCount.FadeOut(300, Easing.OutQuint);
                    playCount.Value = null;
                }

                favouritesStatistic.FadeIn(300, Easing.OutQuint);
                favouritesStatistic.Value = onlineBeatmapSet.FavouriteCount.ToLocalisableString(@"N0");
            }
        }
    }
}
