// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Timing
{
    internal class SampleSection : Section<SampleControlPoint>
    {
        private LabelledTextBox bank;
        private SettingsSlider<int> volume;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                bank = new LabelledTextBox
                {
                    Label = "Bank Name",
                },
                volume = new SettingsSlider<int>
                {
                    Bindable = new SampleControlPoint().SampleVolumeBindable,
                    LabelText = "Volume",
                }
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<SampleControlPoint> point)
        {
            if (point.NewValue != null)
            {
                bank.Current = point.NewValue.SampleBankBindable;
                volume.Bindable = point.NewValue.SampleVolumeBindable;
            }
        }

        protected override SampleControlPoint CreatePoint()
        {
            var reference = Beatmap.Value.Beatmap.ControlPointInfo.SamplePointAt(SelectedGroup.Value.Time);

            return new SampleControlPoint
            {
                SampleBank = reference.SampleBank,
                SampleVolume = reference.SampleVolume,
            };
        }
    }
}
