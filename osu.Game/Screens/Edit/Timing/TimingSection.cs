// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

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

            bpmTextEntry.Bindable.BindValueChanged(val =>
            {
                if (!Beatmap.AdjustNotesOnOffsetBPMChange.Value || ControlPoint.Value == null)
                    return;

                TimingSectionAdjustments.SetHitObjectBPM(Beatmap, ControlPoint.Value, val.OldValue);
                Beatmap.UpdateAllHitObjects();
            });
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

    public static class TimingSectionAdjustments
    {
        public static List<HitObject> HitObjectsInTimingRange(IBeatmap beatmap, double time)
        {
            // If the first group, we grab all hitobjects prior to the next, if the last group, we grab all remaining hitobjects
            double startTime = beatmap.ControlPointInfo.TimingPoints.Any(x => x.Time < time) ? time : double.MinValue;
            double endTime = beatmap.ControlPointInfo.TimingPoints.FirstOrDefault(x => x.Time > time)?.Time ?? double.MaxValue;

            return beatmap.HitObjects.Where(x => Precision.AlmostBigger(x.StartTime, startTime) && Precision.DefinitelyBigger(endTime, x.StartTime)).ToList();
        }

        public static void AdjustHitObjectOffset(IBeatmap beatmap, TimingControlPoint timingControlPoint, double adjust)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap, timingControlPoint.Time))
            {
                hitObject.StartTime += adjust;
            }
        }

        public static void SetHitObjectBPM(IBeatmap beatmap, TimingControlPoint timingControlPoint, double oldBeatLength)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap, timingControlPoint.Time))
            {
                double beat = (hitObject.StartTime - timingControlPoint.Time) / oldBeatLength;

                hitObject.StartTime = (beat * timingControlPoint.BeatLength) + timingControlPoint.Time;

                if (hitObject is not IHasRepeats && hitObject is IHasDuration hitObjectWithDuration)
                    hitObjectWithDuration.Duration *= timingControlPoint.BeatLength / oldBeatLength;
            }
        }
    }
}
