// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class BeatmapOffsetControl : CompositeDrawable
    {
        public Bindable<ScoreInfo?> ReferenceScore { get; } = new Bindable<ScoreInfo?>();

        public BindableDouble Current { get; } = new BindableDouble
        {
            MinValue = -50,
            MaxValue = 50,
            Precision = 0.1,
        };

        private readonly FillFlowContainer referenceScoreContainer;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private double lastPlayAverage;
        private double lastPlayBeatmapOffset;
        private HitEventTimingDistributionGraph? lastPlayGraph;

        private SettingsButton? useAverageButton;

        private IDisposable? beatmapOffsetSubscription;

        private Task? realmWriteTask;

        public BeatmapOffsetControl()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Children = new Drawable[]
                {
                    new OffsetSliderBar
                    {
                        KeyboardStep = 5,
                        LabelText = BeatmapOffsetControlStrings.BeatmapOffset,
                        Current = Current,
                    },
                    referenceScoreContainer = new FillFlowContainer
                    {
                        Spacing = new Vector2(10),
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                }
            };
        }

        public partial class OffsetSliderBar : PlayerSliderBar<double>
        {
            protected override Drawable CreateControl() => new CustomSliderBar();

            protected partial class CustomSliderBar : SliderBar
            {
                public override LocalisableString TooltipText =>
                    Current.Value == 0
                        ? LocalisableString.Interpolate($@"{base.TooltipText} ms")
                        : LocalisableString.Interpolate($@"{base.TooltipText} ms {getEarlyLateText(Current.Value)}");

                private LocalisableString getEarlyLateText(double value)
                {
                    Debug.Assert(value != 0);

                    return value > 0
                        ? BeatmapOffsetControlStrings.HitObjectsAppearEarlier
                        : BeatmapOffsetControlStrings.HitObjectsAppearLater;
                }
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ReferenceScore.BindValueChanged(scoreChanged, true);

            beatmapOffsetSubscription = realm.SubscribeToPropertyChanged(
                r => r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)?.UserSettings,
                settings => settings.Offset,
                val =>
                {
                    // At the point we reach here, it's not guaranteed that all realm writes have taken place (there may be some in-flight).
                    // We are only aware of writes that originated from our own flow, so if we do see one that's active we can avoid handling the feedback value arriving.
                    if (realmWriteTask == null)
                        Current.Value = val;

                    if (realmWriteTask?.IsCompleted == true)
                    {
                        // we can also mark any in-flight write that is managed locally as "seen" and start handling any incoming changes again.
                        realmWriteTask = null;
                    }
                });

            Current.BindValueChanged(currentChanged);
        }

        private void currentChanged(ValueChangedEvent<double> offset)
        {
            Scheduler.AddOnce(updateOffset);

            void updateOffset()
            {
                // the last play graph is relative to the offset at the point of the last play, so we need to factor that out.
                double adjustmentSinceLastPlay = lastPlayBeatmapOffset - Current.Value;

                // Negative is applied here because the play graph is considering a hit offset, not track (as we currently use for clocks).
                lastPlayGraph?.UpdateOffset(-adjustmentSinceLastPlay);

                // ensure the previous write has completed. ignoring performance concerns, if we don't do this, the async writes could be out of sequence.
                if (realmWriteTask?.IsCompleted == false)
                {
                    Scheduler.AddOnce(updateOffset);
                    return;
                }

                if (useAverageButton != null)
                {
                    useAverageButton.Enabled.Value = !Precision.AlmostEquals(lastPlayAverage, adjustmentSinceLastPlay, Current.Precision / 2);
                }

                realmWriteTask = realm.WriteAsync(r =>
                {
                    var settings = r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)?.UserSettings;

                    if (settings == null) // only the case for tests.
                        return;

                    double val = Current.Value;

                    if (settings.Offset == val)
                        return;

                    settings.Offset = val;
                });
            }
        }

        private void scoreChanged(ValueChangedEvent<ScoreInfo?> score)
        {
            referenceScoreContainer.Clear();

            if (score.NewValue == null)
                return;

            if (score.NewValue.Mods.Any(m => !m.UserPlayable))
                return;

            var hitEvents = score.NewValue.HitEvents;

            if (!(hitEvents.CalculateAverageHitError() is double average))
                return;

            referenceScoreContainer.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = BeatmapOffsetControlStrings.PreviousPlay
                },
            };

            if (hitEvents.Count < 10)
            {
                referenceScoreContainer.AddRange(new Drawable[]
                {
                    new OsuTextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Colour = colours.Red1,
                        Text = BeatmapOffsetControlStrings.PreviousPlayTooShortToUseForCalibration
                    },
                });

                return;
            }

            lastPlayAverage = average;
            lastPlayBeatmapOffset = Current.Value;

            referenceScoreContainer.AddRange(new Drawable[]
            {
                lastPlayGraph = new HitEventTimingDistributionGraph(hitEvents)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                },
                new AverageHitError(hitEvents),
                useAverageButton = new SettingsButton
                {
                    Text = BeatmapOffsetControlStrings.CalibrateUsingLastPlay,
                    Action = () => Current.Value = lastPlayBeatmapOffset - lastPlayAverage
                },
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapOffsetSubscription?.Dispose();
        }
    }
}
