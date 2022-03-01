// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class BeatmapOffsetControl : CompositeDrawable
    {
        public Bindable<ScoreInfo> ReferenceScore { get; } = new Bindable<ScoreInfo>();

        public BindableDouble Current { get; } = new BindableDouble
        {
            Default = 0,
            Value = 0,
            MinValue = -50,
            MaxValue = 50,
            Precision = 0.1,
        };

        private SettingsButton useAverageButton;

        private double lastPlayAverage;

        private readonly FillFlowContainer referenceScoreContainer;

        private IDisposable beatmapOffsetSubscription;

        [Resolved]
        private RealmAccess realm { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

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
                    new PlayerSliderBar<double>
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ReferenceScore.BindValueChanged(scoreChanged, true);

            beatmapOffsetSubscription = realm.RegisterCustomSubscription(r =>
            {
                var userSettings = r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)?.UserSettings;

                if (userSettings == null) // only the case for tests.
                    return null;

                Current.Value = userSettings.Offset;
                userSettings.PropertyChanged += onUserSettingsOnPropertyChanged;

                return new InvokeOnDisposal(() => userSettings.PropertyChanged -= onUserSettingsOnPropertyChanged);

                void onUserSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
                {
                    if (args.PropertyName == nameof(BeatmapUserSettings.Offset))
                        Current.Value = userSettings.Offset;
                }
            });

            Current.BindValueChanged(currentChanged);
        }

        private Task realmWrite;

        private void currentChanged(ValueChangedEvent<double> offset)
        {
            Scheduler.AddOnce(updateOffset);

            void updateOffset()
            {
                // ensure the previous write has completed. ignoring performance concerns, if we don't do this, the async writes could be out of sequence.
                if (realmWrite?.IsCompleted == false)
                {
                    Scheduler.AddOnce(updateOffset);
                    return;
                }

                if (useAverageButton != null)
                    useAverageButton.Enabled.Value = !Precision.AlmostEquals(lastPlayAverage, Current.Value, Current.Precision);

                realmWrite = realm.WriteAsync(r =>
                {
                    var settings = r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)?.UserSettings;

                    if (settings == null) // only the case for tests.
                        return;

                    if (Precision.AlmostEquals(settings.Offset, Current.Value))
                        return;

                    settings.Offset = Current.Value;
                });
            }
        }

        private void scoreChanged(ValueChangedEvent<ScoreInfo> score)
        {
            referenceScoreContainer.Clear();

            if (score.NewValue == null)
                return;

            if (score.NewValue.Mods.Any(m => m is ModAutoplay))
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

            referenceScoreContainer.AddRange(new Drawable[]
            {
                new HitEventTimingDistributionGraph(hitEvents)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                },
                new AverageHitError(hitEvents),
                useAverageButton = new SettingsButton
                {
                    Text = BeatmapOffsetControlStrings.CalibrateUsingLastPlay,
                    Action = () => Current.Value = -lastPlayAverage
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
