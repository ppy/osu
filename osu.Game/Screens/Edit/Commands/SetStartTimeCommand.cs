// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetStartTimeCommand : PropertyChangeCommand<HitObject, double>
    {
        public SetStartTimeCommand(HitObject target, double value)
            : base(target, value)
        {
        }

        protected override double ReadValue(HitObject target) => target.StartTime;

        protected override void WriteValue(HitObject target, double value) => target.StartTime = value;

        protected override SetStartTimeCommand CreateInstance(HitObject target, double value) => new SetStartTimeCommand(target, value);
    }
}
