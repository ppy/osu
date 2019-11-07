// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Timing
{
    internal class DifficultySection : Section<DifficultyControlPoint>
    {
        private SettingsSlider<double> multiplier;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new[]
            {
                multiplier = new SettingsSlider<double>
                {
                    LabelText = "Speed Multiplier",
                    Bindable = new DifficultyControlPoint().SpeedMultiplierBindable,
                    RelativeSizeAxes = Axes.X,
                }
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<DifficultyControlPoint> point)
        {
            if (point.NewValue != null)
            {
                multiplier.Bindable = point.NewValue.SpeedMultiplierBindable;
            }
        }

        protected override DifficultyControlPoint CreatePoint()
        {
            var reference = Beatmap.Value.Beatmap.ControlPointInfo.DifficultyPointAt(SelectedGroup.Value.Time);

            return new DifficultyControlPoint
            {
                SpeedMultiplier = reference.SpeedMultiplier,
            };
        }
    }
}
