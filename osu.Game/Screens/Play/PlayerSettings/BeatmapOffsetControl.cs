// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Audio;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class BeatmapOffsetControl : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        public Bindable<ScoreInfo?> ReferenceScore { get; } = new Bindable<ScoreInfo?>();

        private Bindable<ScoreInfo?> lastAppliedScore { get; } = new Bindable<ScoreInfo?>();

        private readonly Bindable<bool> autoAdjustBeatmapOffset = new Bindable<bool>();

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

        [Resolved]
        private Player? player { get; set; }

        [Resolved]
        private SettingsOverlay? settings { get; set; }

        // the last play graph is relative to the offset at the point of the last play, so we need to factor that out for some usages.
        private double adjustmentSinceLastPlay => lastPlayBeatmapOffset - Current.Value;

        private double lastPlayMedian;
        private double lastPlayUnstableRate;
        private double lastPlayBeatmapOffset;
        private HitEventTimingDistributionGraph? lastPlayGraph;
        private SettingsButton? calibrateFromLastPlayButton;
        private IDisposable? beatmapOffsetSubscription;
        private Task? realmWriteTask;
        private ScoreInfo? lastValidScore;

        private bool allowOffsetAdjust => player?.AllowCriticalSettingsAdjustment != false;

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
                        LabelText = BeatmapOffsetControlStrings.AudioOffsetThisBeatmap,
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

        [BackgroundDependencyLoader]
        private void load(SessionStatics statics, OsuConfigManager config)
        {
            statics.BindWith(Static.LastAppliedOffsetScore, lastAppliedScore);
            config.BindWith(OsuSetting.AutomaticallyAdjustBeatmapOffset, autoAdjustBeatmapOffset);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapOffsetSubscription = realm.SubscribeToPropertyChanged(
                r => r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)?.UserSettings,
                settings => settings.Offset,
                val =>
                {
                    // At the point we reach here, it's not guaranteed that all realm writes have taken place (there may be some in-flight).
                    // We are only aware of writes that originated from our own flow, so if we do see one that's active we can avoid handling the feedback value arriving.
                    if (realmWriteTask == null)
                    {
                        Current.Disabled = false;
                        Current.Value = val;
                        Current.Disabled = allowOffsetAdjust;
                    }

                    if (realmWriteTask?.IsCompleted == true)
                    {
                        // we can also mark any in-flight write that is managed locally as "seen" and start handling any incoming changes again.
                        realmWriteTask = null;
                    }
                });

            Current.BindValueChanged(currentChanged);
            ReferenceScore.BindValueChanged(scoreChanged, true);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            // To match stable, this should adjust by 5 ms, or 1 ms when holding alt.
            // But that is hard to make work with global actions due to the operating mode.
            // Let's use the more precise as a default for now.
            const double amount = 1;

            switch (e.Action)
            {
                case GlobalAction.IncreaseOffset:
                    if (!Current.Disabled)
                        Current.Value += amount;
                    return true;

                case GlobalAction.DecreaseOffset:
                    if (!Current.Disabled)
                        Current.Value -= amount;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override void Update()
        {
            base.Update();

            bool allow = allowOffsetAdjust;

            if (calibrateFromLastPlayButton != null)
            {
                double suggestedOffset = computeSuggestedOffset(lastPlayMedian, lastPlayUnstableRate, lastPlayBeatmapOffset);
                calibrateFromLastPlayButton.Enabled.Value = allow && !Precision.AlmostEquals(suggestedOffset, Current.Value, Current.Precision / 2);
            }

            Current.Disabled = !allow;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapOffsetSubscription?.Dispose();
        }

        private void currentChanged(ValueChangedEvent<double> offset)
        {
            // Negative is applied here because the play graph is considering a hit offset, not track (as we currently use for clocks).
            lastPlayGraph?.UpdateOffset(-adjustmentSinceLastPlay);

            // Calibration button may be hidden due to automatic offset adjustment, but it should be visible when the user manually adjusts their offset away from the applied suggestion.
            calibrateFromLastPlayButton?.Show();

            // This is intentionally not scheduled as the offset may be changed while the control is hidden and cannot process its scheduler.
            // This is the case when auto-adjustment is enabled and the offset is adjusted while the player is quick-retrying.
            writeOffsetToBeatmap();
        }

        private void writeOffsetToBeatmap()
        {
            // ensure the previous write has completed. ignoring performance concerns, if we don't do this, the async writes could be out of sequence.
            if (realmWriteTask?.IsCompleted == false)
            {
                Scheduler.AddOnce(writeOffsetToBeatmap);
                return;
            }

            realmWriteTask = realm.WriteAsync(r =>
            {
                var setInfo = r.Find<BeatmapSetInfo>(beatmap.Value.BeatmapSetInfo.ID);

                if (setInfo == null) // only the case for tests.
                    return;

                // Apply to all difficulties in a beatmap set if they have the same audio
                // (they generally always share timing).
                foreach (var b in setInfo.Beatmaps)
                {
                    BeatmapUserSettings userSettings = b.UserSettings;
                    double val = Current.Value;

                    if (userSettings.Offset != val && b.AudioEquals(beatmap.Value.BeatmapInfo))
                        userSettings.Offset = val;
                }
            });
        }

        private void scoreChanged(ValueChangedEvent<ScoreInfo?> score)
        {
            if (score.NewValue == null)
                return;

            if (score.NewValue.Equals(lastAppliedScore.Value))
                return;

            if (!score.NewValue.BeatmapInfo.AsNonNull().Equals(beatmap.Value.BeatmapInfo))
                return;

            if (score.NewValue.Mods.Any(m => !m.UserPlayable || m is IHasNoTimedInputs))
                return;

            var hitEvents = score.NewValue.HitEvents;

            if (hitEvents.CalculateMedianHitError() is not double median)
                return;

            if (hitEvents.CalculateUnstableRate()?.Result is not double unstableRate)
                return;

            // affecting unstable rate here is used as a substitute of determining if a hit event represents a *timed* hit event,
            // i.e. a user input that the user had to *time to the track*,
            // i.e. one that it *makes sense to use* when doing anything with timing and offsets.
            bool hasEnoughUsableEvents = hitEvents.Count(HitEventExtensions.AffectsUnstableRate) >= 50;

            // If we already have an old score with enough hit events and the new score doesn't have enough, continue displaying the old one rather than showing the user "play too short" message.
            if (lastValidScore != null && !hasEnoughUsableEvents)
                return;

            referenceScoreContainer.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = BeatmapOffsetControlStrings.PreviousPlay
                },
            };

            if (!hasEnoughUsableEvents)
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

            lastValidScore = score.NewValue!;
            lastPlayMedian = median;
            lastPlayUnstableRate = unstableRate;
            lastPlayBeatmapOffset = Current.Value;

            LinkFlowContainer offsetText;

            referenceScoreContainer.AddRange(new Drawable[]
            {
                lastPlayGraph = new HitEventTimingDistributionGraph(hitEvents)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                },
                new AverageHitError(hitEvents) { FontSize = OsuFont.Style.Caption1.Size },
                calibrateFromLastPlayButton = new SettingsButton
                {
                    Text = BeatmapOffsetControlStrings.CalibrateUsingLastPlay,
                    Action = () =>
                    {
                        if (!Current.Disabled)
                            applySuggestedOffset();
                    },
                },
                offsetText = new LinkFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            });

            if (autoAdjustBeatmapOffset.Value && !Current.Disabled)
            {
                bool offsetChanged = applySuggestedOffset();

                calibrateFromLastPlayButton.Hide();

                if (offsetChanged)
                {
                    offsetText.AddText($"Beatmap offset was adjusted to {Current.Value.ToStandardFormattedString(1)} ms.", t => t.Font = OsuFont.Style.Caption1);
                    offsetText.NewParagraph();
                }
            }

            offsetText.AddText("You can also ", t => t.Font = OsuFont.Style.Caption2);
            offsetText.AddLink("adjust the global offset", () => settings?.ShowAtControl<AudioOffsetAdjustControl>(), creationParameters: t => t.Font = OsuFont.Style.Caption2);
            offsetText.AddText(" based off this play.", t => t.Font = OsuFont.Style.Caption2);
        }

        private bool applySuggestedOffset()
        {
            double lastOffset = Current.Value;

            Current.Value = computeSuggestedOffset(lastPlayMedian, lastPlayUnstableRate, lastPlayBeatmapOffset);
            lastAppliedScore.Value = lastValidScore;

            return !Precision.AlmostEquals(Current.Value, lastOffset, Current.Precision / 2);
        }

        public static LocalisableString GetOffsetExplanatoryText(double offset)
        {
            string formatOffset = offset.ToStandardFormattedString(1);

            return formatOffset == "0"
                ? LocalisableString.Interpolate($@"{formatOffset} ms")
                : LocalisableString.Interpolate($@"{formatOffset} ms {getEarlyLateText(offset)}");

            LocalisableString getEarlyLateText(double value)
            {
                Debug.Assert(value != 0);

                return value > 0
                    ? BeatmapOffsetControlStrings.HitObjectsAppearEarlier
                    : BeatmapOffsetControlStrings.HitObjectsAppearLater;
            }
        }

        private static double computeSuggestedOffset(double median, double unstableRate, double currentOffset)
        {
            const double ur_adjustment_cutoff = 90;
            const double exponential_factor = -0.0116;

            double offsetAdjustment = median;

            if (unstableRate >= ur_adjustment_cutoff)
            {
                // A demonstrative graph of this algorithm is embedded in https://github.com/ppy/osu/discussions/30521.
                // This ultimately prevents scores with high unstable rate from suggesting potentially invalid offsets.
                offsetAdjustment *= Math.Exp(exponential_factor * (unstableRate - ur_adjustment_cutoff));
            }

            return currentOffset - offsetAdjustment;
        }

        private partial class OffsetSliderBar : PlayerSliderBar<double>
        {
            protected override Drawable CreateControl() => new CustomSliderBar();

            protected partial class CustomSliderBar : SliderBar
            {
                public override LocalisableString TooltipText => GetOffsetExplanatoryText(Current.Value);
            }
        }
    }
}
