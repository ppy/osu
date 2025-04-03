// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Timing
{
    internal partial class TimingSection : Section<TimingControlPoint>
    {
        private LabelledTimeSignature timeSignature = null!;
        private LabelledSwitchButton omitBarLine = null!;
        private BPMTextBox bpmTextEntry = null!;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                new LabelledSwitchButton
                {
                    Label = EditorStrings.AdjustExistingObjectsOnTimingChanges,
                    FixedLabelWidth = 220,
                    Current = configManager.GetBindable<bool>(OsuSetting.EditorAdjustExistingObjectsOnTimingChanges),
                },
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

            bpmTextEntry.OnCommit = (oldBeatLength, _) =>
            {
                if (!configManager.Get<bool>(OsuSetting.EditorAdjustExistingObjectsOnTimingChanges) || ControlPoint.Value == null)
                    return;

                Beatmap.BeginChange();
                TimingSectionAdjustments.SetHitObjectBPM(Beatmap, ControlPoint.Value, oldBeatLength);
                Beatmap.UpdateAllHitObjects();
                Beatmap.EndChange();
            };
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
            public new Action<double, double>? OnCommit { get; set; }

            private readonly BindableNumber<double> beatLengthBindable = new TimingControlPoint().BeatLengthBindable;

            public BPMTextBox()
            {
                Label = "BPM";
                SelectAllOnFocus = true;

                base.OnCommit += (_, isNew) =>
                {
                    if (!isNew) return;

                    double oldBeatLength = beatLengthBindable.Value;

                    try
                    {
                        if (double.TryParse(Current.Value, out double doubleVal) && doubleVal > 0)
                            beatLengthBindable.Value = BeatLengthToBpm(doubleVal);
                    }
                    catch
                    {
                        // TriggerChange below will restore the previous text value on failure.
                    }

                    // This is run regardless of parsing success as the parsed number may not actually trigger a change
                    // due to bindable clamping. Even in such a case we want to update the textbox to a sane visual state.
                    beatLengthBindable.TriggerChange();
                    OnCommit?.Invoke(oldBeatLength, beatLengthBindable.Value);
                };

                beatLengthBindable.BindValueChanged(val =>
                {
                    Current.Value = BeatLengthToBpm(val.NewValue).ToString("N2");
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

        public static double BeatLengthToBpm(double beatLength) => 60000 / beatLength;
    }
}
