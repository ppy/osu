// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        private SettingsSlider<double> bpm;
        private SettingsEnumDropdown<TimeSignatures> timeSignature;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                bpm = new BPMSlider
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
                bpm.Bindable = point.NewValue.BeatLengthBindable;
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
            private readonly BindableDouble beatLengthBindable = new BindableDouble();

            private BindableDouble bpmBindable;

            public override Bindable<double> Bindable
            {
                get => base.Bindable;
                set
                {
                    // incoming will be beatlength

                    beatLengthBindable.UnbindBindings();
                    beatLengthBindable.BindTo(value);

                    base.Bindable = bpmBindable = new BindableDouble(beatLengthToBpm(beatLengthBindable.Value))
                    {
                        MinValue = beatLengthToBpm(beatLengthBindable.MaxValue),
                        MaxValue = beatLengthToBpm(beatLengthBindable.MinValue),
                        Default = beatLengthToBpm(beatLengthBindable.Default),
                    };

                    bpmBindable.BindValueChanged(bpm => beatLengthBindable.Value = beatLengthToBpm(bpm.NewValue));
                }
            }

            private double beatLengthToBpm(double beatLength) => 60000 / beatLength;
        }
    }
}
