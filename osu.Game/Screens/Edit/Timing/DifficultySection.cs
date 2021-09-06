// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;

namespace osu.Game.Screens.Edit.Timing
{
    internal class DifficultySection : Section<DifficultyControlPoint>
    {
        private SliderWithTextBoxInput<double> sliderVelocitySlider;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new[]
            {
                sliderVelocitySlider = new SliderWithTextBoxInput<double>("Slider Velocity")
                {
                    Current = new DifficultyControlPoint().SliderVelocityBindable,
                    KeyboardStep = 0.1f
                }
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<DifficultyControlPoint> point)
        {
            if (point.NewValue != null)
            {
                var selectedPointBindable = point.NewValue.SliderVelocityBindable;

                // there may be legacy control points, which contain infinite precision for compatibility reasons (see LegacyDifficultyControlPoint).
                // generally that level of precision could only be set by externally editing the .osu file, so at the point
                // a user is looking to update this within the editor it should be safe to obliterate this additional precision.
                double expectedPrecision = new DifficultyControlPoint().SliderVelocityBindable.Precision;
                if (selectedPointBindable.Precision < expectedPrecision)
                    selectedPointBindable.Precision = expectedPrecision;

                sliderVelocitySlider.Current = selectedPointBindable;
                sliderVelocitySlider.Current.BindValueChanged(_ => ChangeHandler?.SaveState());
            }
        }

        protected override DifficultyControlPoint CreatePoint()
        {
            var reference = (Beatmap.ControlPointInfo as LegacyControlPointInfo)?.DifficultyPointAt(SelectedGroup.Value.Time) ?? DifficultyControlPoint.DEFAULT;

            return new DifficultyControlPoint
            {
                SliderVelocity = reference.SliderVelocity,
            };
        }
    }
}
