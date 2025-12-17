// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    public class StartTimeChange : PropertyChange<HitObject, double>
    {
        public StartTimeChange(HitObject target, double value)
            : base(target, value)
        {
        }

        protected override double ReadValue(HitObject target) => target.StartTime;

        protected override void WriteValue(HitObject target, double value) => target.StartTime = value;
    }
}
