// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Timing
{
    internal partial class TimingSection : Section<TimingControlPoint>
    {
        private LabelledTimeSignature timeSignature = null!;
        private LabelledSwitchButton omitBarLine = null!;
        private BPMTextBox bpmTextEntry = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                new TapTimingControl(),
                bpmTextEntry = new BPMTextBox(),
                timeSignature = new LabelledTimeSignature
                {
                    Label = "Time Signature"
                },
                omitBarLine = new LabelledSwitchButton { Label = "Skip Bar Line" },
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            bpmTextEntry.Current.BindValueChanged(_ => saveChanges());
            omitBarLine.Current.BindValueChanged(_ => saveChanges());
            timeSignature.Current.BindValueChanged(_ => saveChanges());

            void saveChanges()
            {
                if (!isRebinding) ChangeHandler?.SaveState();
            }
        }

        private bool isRebinding;

        protected override void OnControlPointChanged(ValueChangedEvent<TimingControlPoint?> point)
        {
            if (point.NewValue != null)
            {
                isRebinding = true;

                bpmTextEntry.Bindable = point.NewValue.BeatLengthBindable;
                timeSignature.Current = point.NewValue.TimeSignatureBindable;
                omitBarLine.Current = point.NewValue.OmitFirstBarLineBindable;

                isRebinding = false;
            }
        }

        protected override TimingControlPoint CreatePoint()
        {
            var reference = Beatmap.ControlPointInfo.TimingPointAt(SelectedGroup.Value.Time);

            return new TimingControlPoint
            {
                BeatLength = reference.BeatLength,
                TimeSignature = reference.TimeSignature,
                OmitFirstBarLine = reference.OmitFirstBarLine,
            };
        }

        private partial class BPMTextBox : LabelledTextBox
        {
            private readonly BindableNumber<double> beatLengthBindable = new TimingControlPoint().BeatLengthBindable;

            public BPMTextBox()
            {
                Label = "BPM";

                OnCommit += (_, isNew) =>
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

        private static double beatLengthToBpm(double beatLength) => 60000 / beatLength;
    }
}
