// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Timing
{
    internal class SampleSection : Section<SampleControlPoint>
    {
        private LabelledTextBox bank;
        private SliderWithTextBoxInput<int> volume;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                bank = new LabelledTextBox
                {
                    Label = "Bank Name",
                },
                volume = new SliderWithTextBoxInput<int>("Volume")
                {
                    Current = new SampleControlPoint().SampleVolumeBindable,
                }
            });
        }

        protected override void OnControlPointChanged(ValueChangedEvent<SampleControlPoint> point)
        {
            if (point.NewValue != null)
            {
                bank.Current = point.NewValue.SampleBankBindable;
                bank.Current.BindValueChanged(_ => ChangeHandler?.SaveState());

                volume.Current = point.NewValue.SampleVolumeBindable;
                volume.Current.BindValueChanged(_ => ChangeHandler?.SaveState());
            }
        }

        protected override SampleControlPoint CreatePoint() => new SampleControlPoint(); // TODO: remove
    }
}
