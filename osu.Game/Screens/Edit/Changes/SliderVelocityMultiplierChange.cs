// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Changes
{
    public class SliderVelocityMultiplierChange : PropertyChange<IHasSliderVelocity, double>
    {
        public SliderVelocityMultiplierChange(IHasSliderVelocity target, double value)
            : base(target, value)
        {
        }

        protected override double ReadValue(IHasSliderVelocity target) => target.SliderVelocityMultiplier;

        protected override void WriteValue(IHasSliderVelocity target, double value) => target.SliderVelocityMultiplier = value;
    }
}
