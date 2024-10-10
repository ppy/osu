// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetSliderVelocityMultiplierCommand : PropertyChangeCommand<IHasSliderVelocity, double>
    {
        public SetSliderVelocityMultiplierCommand(IHasSliderVelocity target, double value)
            : base(target, value)
        {
        }

        protected override double ReadValue(IHasSliderVelocity target) => target.SliderVelocityMultiplier;

        protected override void WriteValue(IHasSliderVelocity target, double value) => target.SliderVelocityMultiplier = value;

        protected override SetSliderVelocityMultiplierCommand CreateInstance(IHasSliderVelocity target, double value) => new SetSliderVelocityMultiplierCommand(target, value);

        protected override bool ValueEquals(double a, double b) => Precision.AlmostEquals(a, b);
    }
}
