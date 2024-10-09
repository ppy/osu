// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public class MoveXCommand : IEditorCommand
    {
        public readonly IHasMutablePosition Target;

        public readonly float X;

        public MoveXCommand(IHasMutablePosition target, float x)
        {
            Target = target;
            X = x;
        }

        public void Apply() => Target.X = X;

        public IEditorCommand CreateUndo() => new MoveXCommand(Target, Target.X);

        public bool IsRedundant => Precision.AlmostEquals(X, Target.X);
    }
}
