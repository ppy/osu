// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public class MoveYCommand : IEditorCommand
    {
        public readonly IHasMutablePosition Target;

        public readonly float Y;

        public MoveYCommand(IHasMutablePosition target, float y)
        {
            Target = target;
            Y = y;
        }

        public void Apply() => Target.Y = Y;

        public IEditorCommand CreateUndo() => new MoveXCommand(Target, Target.Y);

        public bool IsRedundant => Precision.AlmostEquals(Y, Target.Y);
    }
}
