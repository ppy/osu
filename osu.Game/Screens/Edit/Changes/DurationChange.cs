// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Changes
{
    public class DurationChange : PropertyChange<IHasDuration, double>
    {
        public DurationChange(IHasDuration target, double value)
            : base(target, value)
        {
        }

        protected override double ReadValue(IHasDuration target) => target.Duration;

        protected override void WriteValue(IHasDuration target, double value) => target.Duration = value;
    }
}
