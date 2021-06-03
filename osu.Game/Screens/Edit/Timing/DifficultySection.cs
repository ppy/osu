// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Timing
{
    internal class DifficultySection : Section<DifficultyControlPoint>
    {
        private SliderWithTextBoxInput<double> multiplierSlider;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new[]
            {
                multiplierSlider = new SliderWithTextBoxInput<double>("Speed Multiplier")
                {
                    Current = new DifficultyControlPoint().SpeedMultiplierBindable,
                    KeyboardStep = 0.1f
                }
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<DifficultyControlPoint> point)
        {
            if (point.NewValue != null)
            {
                var selectedPointBindable = point.NewValue.SpeedMultiplierBindable;

                // there may be legacy control points, which contain infinite precision for compatibility reasons (see LegacyDifficultyControlPoint).
                // generally that level of precision could only be set by externally editing the .osu file, so at the point
                // a user is looking to update this within the editor it should be safe to obliterate this additional precision.
                double expectedPrecision = new DifficultyControlPoint().SpeedMultiplierBindable.Precision;
                if (selectedPointBindable.Precision < expectedPrecision)
                    selectedPointBindable.Precision = expectedPrecision;

                multiplierSlider.Current = selectedPointBindable;
                multiplierSlider.Current.BindValueChanged(_ => ChangeHandler?.SaveState());
            }
        }

        protected override DifficultyControlPoint CreatePoint()
        {
            var reference = Beatmap.ControlPointInfo.DifficultyPointAt(SelectedGroup.Value.Time);

            return new DifficultyControlPoint
            {
                SpeedMultiplier = reference.SpeedMultiplier,
            };
        }
    }
}
