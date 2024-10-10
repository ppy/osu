// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetExpectedDistanceCommand : PropertyChangeCommand<SliderPath, double?>
    {
        public SetExpectedDistanceCommand(SliderPath target, double? value)
            : base(target, value)
        {
        }

        protected override double? ReadValue(SliderPath target) => target.ExpectedDistance.Value;

        protected override void WriteValue(SliderPath target, double? value) => target.ExpectedDistance.Value = value;

        protected override SetExpectedDistanceCommand CreateInstance(SliderPath target, double? value) => new SetExpectedDistanceCommand(target, value);
    }
}
