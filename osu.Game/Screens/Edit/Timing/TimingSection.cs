// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Timing
{
    internal class TimingSection : Section<TimingControlPoint>
    {
        private SettingsSlider<double> bpmSlider;
        private LabelledTimeSignature timeSignature;
        private BPMTextBox bpmTextEntry;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                bpmTextEntry = new BPMTextBox(),
                bpmSlider = new BPMSlider(),
                timeSignature = new LabelledTimeSignature
                {
                    Label = "Time Signature"
                }
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<TimingControlPoint> point)
        {
            if (point.NewValue != null)
            {
                bpmSlider.Current = point.NewValue.BeatLengthBindable;
                bpmSlider.Current.BindValueChanged(_ => ChangeHandler?.SaveState());

                bpmTextEntry.Bindable = point.NewValue.BeatLengthBindable;
                // no need to hook change handler here as it's the same bindable as above

                timeSignature.Current = point.NewValue.TimeSignatureBindable;
                timeSignature.Current.BindValueChanged(_ => ChangeHandler?.SaveState());
            }
        }

        protected override TimingControlPoint CreatePoint()
        {
            var reference = Beatmap.ControlPointInfo.TimingPointAt(SelectedGroup.Value.Time);

            return new TimingControlPoint
            {
                BeatLength = reference.BeatLength,
                TimeSignature = reference.TimeSignature
            };
        }

        private class BPMTextBox : LabelledTextBox
        {
            private readonly BindableNumber<double> beatLengthBindable = new TimingControlPoint().BeatLengthBindable;

            public BPMTextBox()
            {
                Label = "BPM";

                OnCommit += (val, isNew) =>
                {
                    if (!isNew) return;

                    try
                    {
                        if (double.TryParse(Current.Value, out double doubleVal) && doubleVal > 0)
                            beatLengthBindable.Value = beatLengthToBpm(doubleVal);
                    }
                    catch
                    {
                        // TriggerChange below will restore the previous text value on failure.
                    }

                    // This is run regardless of parsing success as the parsed number may not actually trigger a change
                    // due to bindable clamping. Even in such a case we want to update the textbox to a sane visual state.
                    beatLengthBindable.TriggerChange();
                };

                beatLengthBindable.BindValueChanged(val =>
                {
                    Current.Value = beatLengthToBpm(val.NewValue).ToString("N2");
                }, true);
            }

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
            private const double sane_maximum = 240;

            private readonly BindableNumber<double> beatLengthBindable = new TimingControlPoint().BeatLengthBindable;

            private readonly BindableDouble bpmBindable = new BindableDouble(60000 / TimingControlPoint.DEFAULT_BEAT_LENGTH)
            {
                MinValue = sane_minimum,
                MaxValue = sane_maximum,
            };

            public BPMSlider()
            {
                beatLengthBindable.BindValueChanged(beatLength => updateCurrent(beatLengthToBpm(beatLength.NewValue)), true);
                bpmBindable.BindValueChanged(bpm => beatLengthBindable.Value = beatLengthToBpm(bpm.NewValue));

                base.Current = bpmBindable;

                TransferValueOnCommit = true;
            }

            public override Bindable<double> Current
            {
                get => base.Current;
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
