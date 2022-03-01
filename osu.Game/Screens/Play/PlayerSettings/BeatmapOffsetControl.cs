// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class BeatmapOffsetControl : CompositeDrawable
    {
        private readonly SettingsButton useAverageButton;

        private readonly double lastPlayAverage;

        public Bindable<double> Current { get; } = new BindableDouble
        {
            Default = 0,
            Value = 0,
            MinValue = -50,
            MaxValue = 50,
            Precision = 0.1,
        };

        public BeatmapOffsetControl(IReadOnlyList<HitEvent> hitEvents)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            FillFlowContainer flow;

            InternalChild = flow = new FillFlowContainer
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
                }
            };

            if (hitEvents.CalculateAverageHitError() is double average)
            {
                lastPlayAverage = average;

                flow.AddRange(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = "Last play:"
                    },
                    new HitEventTimingDistributionGraph(hitEvents)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                    },
                    new AverageHitError(hitEvents),
                    useAverageButton = new SettingsButton
                    {
                        Text = "Calibrate using last play",
                        Action = () => Current.Value = lastPlayAverage
                    },
                });
            }

            Current.BindValueChanged(offset =>
            {
                if (useAverageButton != null)
                {
                    useAverageButton.Enabled.Value = offset.NewValue != lastPlayAverage;
                }
            }, true);
        }
    }
}
