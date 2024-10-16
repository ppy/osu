// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    public class ExpectedDistanceChange : PropertyChange<SliderPath, double?>
    {
        public ExpectedDistanceChange(SliderPath target, double? value)
            : base(target, value)
        {
        }

        protected override double? ReadValue(SliderPath target) => target.ExpectedDistance.Value;

        protected override void WriteValue(SliderPath target, double? value) => target.ExpectedDistance.Value = value;
    }
}
