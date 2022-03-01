// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class BeatmapOffsetControl : CompositeDrawable
    {
        public Bindable<ScoreInfo> ReferenceScore { get; } = new Bindable<ScoreInfo>();

        public Bindable<double> Current { get; } = new BindableDouble
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
                        LabelText = "Beatmap offset",
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

        [Resolved]
        private RealmAccess realm { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ReferenceScore.BindValueChanged(scoreChanged, true);

            Current.BindValueChanged(currentChanged);
            Current.Value = realm.Run(r => r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID).UserSettings?.Offset) ?? 0;
        }

        private void currentChanged(ValueChangedEvent<double> offset)
        {
            if (useAverageButton != null)
            {
                useAverageButton.Enabled.Value = offset.NewValue != lastPlayAverage;
            }

            realm.Write(r =>
            {
                var settings = r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID).UserSettings;

                settings.Offset = offset.NewValue;
            });
        }

        private void scoreChanged(ValueChangedEvent<ScoreInfo> score)
        {
            if (!(score.NewValue?.HitEvents.CalculateAverageHitError() is double average))
            {
                referenceScoreContainer.Clear();
                return;
            }

            lastPlayAverage = average;

            referenceScoreContainer.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "Last play:"
                },
                new HitEventTimingDistributionGraph(score.NewValue.HitEvents)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                },
                new AverageHitError(score.NewValue.HitEvents),
                useAverageButton = new SettingsButton
                {
                    Text = "Calibrate using last play",
                    Action = () => Current.Value = lastPlayAverage
                },
            };
        }
    }
}
