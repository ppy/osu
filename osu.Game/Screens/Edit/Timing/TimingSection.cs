// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Timing
{
    internal class TimingSection : Section<TimingControlPoint>
    {
        private SettingsSlider<double> bpmSlider;
        private SettingsEnumDropdown<TimeSignatures> timeSignature;
        private BPMTextBox bpmTextEntry;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                bpmTextEntry = new BPMTextBox
                {
                    Bindable = new TimingControlPoint().BeatLengthBindable,
                    Label = "BPM",
                },
                bpmSlider = new BPMSlider
                {
                    Bindable = new TimingControlPoint().BeatLengthBindable,
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
                bpmTextEntry.Bindable = point.NewValue.BeatLengthBindable;
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

        private class BPMTextBox : LabelledTextBox
        {
            public BPMTextBox()
            {
                OnCommit += (val, isNew) =>
                {
                    if (!isNew) return;

                    if (double.TryParse(Current.Value, out double doubleVal))
                    {
                        try
                        {
                            beatLengthBindable.Value = beatLengthToBpm(doubleVal);
                        }
                        catch
                        {
                            // will restore the previous text value on failure.
                            beatLengthBindable.TriggerChange();
                        }
                    }
                };

                beatLengthBindable.BindValueChanged(val =>
                {
                    Current.Value = beatLengthToBpm(val.NewValue).ToString();
                });
            }

            private readonly BindableDouble beatLengthBindable = new BindableDouble();

            public Bindable<double> Bindable
            {
                get => beatLengthBindable;
                set
                {
                    // incoming will be beat length, not bpm
                    beatLengthBindable.UnbindBindings();
                    beatLengthBindable.BindTo(value);
                }
            }
        }

        private class BPMSlider : SettingsSlider<double>
        {
            private const double sane_minimum = 60;
            private const double sane_maximum = 200;

            private readonly BindableDouble beatLengthBindable = new BindableDouble();
            private readonly BindableDouble bpmBindable = new BindableDouble();

            public BPMSlider()
            {
                beatLengthBindable.BindValueChanged(beatLength => updateCurrent(beatLengthToBpm(beatLength.NewValue)));
                bpmBindable.BindValueChanged(bpm => bpmBindable.Default = beatLengthBindable.Value = beatLengthToBpm(bpm.NewValue));

                base.Bindable = bpmBindable;
            }

            public override Bindable<double> Bindable
            {
                get => base.Bindable;
                set
                {
                    // incoming will be beat length, not bpm
                    beatLengthBindable.UnbindBindings();
                    beatLengthBindable.BindTo(value);
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
        }

        private static double beatLengthToBpm(double beatLength) => 60000 / beatLength;
    }
}
