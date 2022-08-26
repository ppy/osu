// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Timing
{
    internal class EffectSection : Section<EffectControlPoint>
    {
        private LabelledSwitchButton kiai;
        private LabelledSwitchButton omitBarLine;

        private SliderWithTextBoxInput<double> scrollSpeedSlider;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                kiai = new LabelledSwitchButton { Label = "Kiai Time" },
                omitBarLine = new LabelledSwitchButton { Label = "Skip Bar Line" },
                scrollSpeedSlider = new SliderWithTextBoxInput<double>("Scroll Speed")
                {
                    Current = new EffectControlPoint().ScrollSpeedBindable,
                    KeyboardStep = 0.1f
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            kiai.Current.BindValueChanged(_ => saveChanges());
            omitBarLine.Current.BindValueChanged(_ => saveChanges());
            scrollSpeedSlider.Current.BindValueChanged(_ => saveChanges());

            void saveChanges()
            {
                if (!isRebinding) ChangeHandler?.SaveState();
            }
        }

        private bool isRebinding;

        protected override void OnControlPointChanged(ValueChangedEvent<EffectControlPoint> point)
        {
            if (point.NewValue != null)
            {
                isRebinding = true;

                kiai.Current = point.NewValue.KiaiModeBindable;
                omitBarLine.Current = point.NewValue.OmitFirstBarLineBindable;
                scrollSpeedSlider.Current = point.NewValue.ScrollSpeedBindable;

                isRebinding = false;
            }
        }

        protected override EffectControlPoint CreatePoint()
        {
            var reference = Beatmap.ControlPointInfo.EffectPointAt(SelectedGroup.Value.Time);

            return new EffectControlPoint
            {
                KiaiMode = reference.KiaiMode,
                OmitFirstBarLine = reference.OmitFirstBarLine,
                ScrollSpeed = reference.ScrollSpeed,
            };
        }
    }
}
