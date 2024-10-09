// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class SetSliderVelocityMultiplierCommand : IEditorCommand
    {
        public readonly Slider Slider;

        public readonly double SliderVelocityMultiplier;

        public SetSliderVelocityMultiplierCommand(Slider slider, double sliderVelocityMultiplier)
        {
            Slider = slider;
            SliderVelocityMultiplier = sliderVelocityMultiplier;
        }

        public void Apply() => Slider.SliderVelocityMultiplier = SliderVelocityMultiplier;

        public IEditorCommand CreateUndo() => new SetSliderVelocityMultiplierCommand(Slider, Slider.SliderVelocityMultiplier);
    }
}
