// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Timing
{
    internal partial class TimingSection : Section<TimingControlPoint>
    {
        private readonly BindableNumberWithCurrent<double> currentBeatLength = new BindableNumberWithCurrent<double>();

        private LabelledTimeSignature timeSignature = null!;
        private LabelledSwitchButton omitBarLine = null!;
        private FormDiscreteAdjustmentControl<double> bpmAdjustmentBox = null!;

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
                bpmAdjustmentBox = new FormDiscreteAdjustmentControl<double>(1)
                {
                    Caption = "BPM",
                    LabelFormat = v => v.ToLocalisableString(@"N2"),
                },
                new TapTimingControl(),
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

            omitBarLine.Current.BindValueChanged(_ => saveChanges());
            timeSignature.Current.BindValueChanged(_ => saveChanges());

            bpmAdjustmentBox.Current.BindValueChanged(val =>
            {
                if (val.NewValue < 0)
                {
                    bpmAdjustmentBox.Current.Value = val.OldValue;
                    return;
                }

                currentBeatLength.Value = BeatLengthToBpm(val.NewValue);
            });
            currentBeatLength.BindValueChanged(val =>
            {
                if (ControlPoint.Value == null)
                    return;

                bpmAdjustmentBox.Current.Value = BeatLengthToBpm(val.NewValue);

                if (isRebinding)
                    return;

                if (val.OldValue != val.NewValue && configManager.Get<bool>(OsuSetting.EditorAdjustExistingObjectsOnTimingChanges))
                {
                    Beatmap.BeginChange();
                    TimingSectionAdjustments.SetHitObjectBPM(Beatmap, ControlPoint.Value, val.OldValue);
                    Beatmap.UpdateAllHitObjects();
                    Beatmap.EndChange();
                }
            }, true);

            void saveChanges()
            {
                if (!isRebinding) ChangeHandler?.SaveState();
            }
        }

        private bool isRebinding;

        protected override void OnSelectedGroupChanged(ValueChangedEvent<ControlPointGroup?> group)
        {
            Checkbox.Current.Disabled = false;
            base.OnSelectedGroupChanged(group);
            // The first control point group is expected to contain timing information at all times.
            Checkbox.Current.Disabled = group.NewValue?.Equals(Beatmap.ControlPointInfo.Groups.FirstOrDefault()) == true;
        }

        protected override void OnControlPointChanged(ValueChangedEvent<TimingControlPoint?> point)
        {
            if (point.NewValue != null)
            {
                isRebinding = true;

                currentBeatLength.Current = point.NewValue.BeatLengthBindable;
                timeSignature.Current = point.NewValue.TimeSignatureBindable;
                omitBarLine.Current = point.NewValue.OmitFirstBarLineBindable;

                isRebinding = false;
            }
        }

        protected override TimingControlPoint CreatePoint(ControlPointGroup selectedGroup)
        {
            var reference = Beatmap.ControlPointInfo.TimingPointAt(selectedGroup.Time);

            return new TimingControlPoint
            {
                BeatLength = reference.BeatLength,
                TimeSignature = reference.TimeSignature,
                OmitFirstBarLine = reference.OmitFirstBarLine,
            };
        }

        public static double BeatLengthToBpm(double beatLength) => 60000 / beatLength;
    }
}
