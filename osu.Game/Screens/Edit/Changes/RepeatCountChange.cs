// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Changes
{
    public class RepeatCountChange : PropertyChange<IHasRepeats, int>
    {
        public RepeatCountChange(IHasRepeats target, int value)
            : base(target, value)
        {
        }

        protected override int ReadValue(IHasRepeats target) => target.RepeatCount;

        protected override void WriteValue(IHasRepeats target, int value) => target.RepeatCount = value;
    }
}
