// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetStartTimeCommand : IEditorCommand
    {
        public readonly HitObject Target;

        public readonly double StartTime;

        public SetStartTimeCommand(HitObject target, double startTime)
        {
            Target = target;
            StartTime = startTime;
        }

        public void Apply() => Target.StartTime = StartTime;

        public IEditorCommand CreateUndo() => new SetStartTimeCommand(Target, Target.StartTime);

        public bool IsRedundant => Precision.AlmostEquals(StartTime, Target.StartTime);
    }
}
