// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Timing
{
    internal class TimingSection : Section<TimingControlPoint>
    {
        private SettingsSlider<double> bpmSlider;
        private SettingsEnumDropdown<TimeSignatures> timeSignature;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                bpmSlider = new BPMSlider
                {
                    Bindable = new TimingControlPoint().BeatLengthBindable,
                    LabelText = "BPM",
                },
                timeSignature = new SettingsEnumDropdown<TimeSignatures>
                {
                    LabelText = "Time Signature"
                },
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<TimingControlPoint> point)
        {
            if (point.NewValue != null)
            {
                bpmSlider.Bindable = point.NewValue.BeatLengthBindable;
                timeSignature.Bindable = point.NewValue.TimeSignatureBindable;
            }
        }

        protected override TimingControlPoint CreatePoint()
        {
            var reference = Beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(SelectedGroup.Value.Time);

            return new TimingControlPoint
            {
                BeatLength = reference.BeatLength,
                TimeSignature = reference.TimeSignature
            };
        }

        private class BPMSlider : SettingsSlider<double>
        {
            private const double sane_minimum = 60;
            private const double sane_maximum = 200;

            private readonly BindableDouble beatLengthBindable = new BindableDouble();

            private BindableDouble bpmBindable;

            public override Bindable<double> Bindable
            {
                get => base.Bindable;
                set
                {
                    // incoming will be beat length, not bpm
                    beatLengthBindable.UnbindBindings();
                    beatLengthBindable.BindTo(value);

                    double initial = beatLengthToBpm(beatLengthBindable.Value);

                    bpmBindable = new BindableDouble(initial)
                    {
                        Default = beatLengthToBpm(beatLengthBindable.Default),
                    };

                    updateCurrent(initial);

                    bpmBindable.BindValueChanged(bpm =>
                    {
                        updateCurrent(bpm.NewValue);
                        beatLengthBindable.Value = beatLengthToBpm(bpm.NewValue);
                    });

                    base.Bindable = bpmBindable;
                }
            }

            private void updateCurrent(double newValue)
            {
                // we use a more sane range for the slider display unless overridden by the user.
                // if a value comes in outside our range, we should expand temporarily.
                bpmBindable.MinValue = Math.Min(newValue, sane_minimum);
                bpmBindable.MaxValue = Math.Max(newValue, sane_maximum);

                bpmBindable.Value = newValue;
            }

            private double beatLengthToBpm(double beatLength) => 60000 / beatLength;
        }
    }
}
