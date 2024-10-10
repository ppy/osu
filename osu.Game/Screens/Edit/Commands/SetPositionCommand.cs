// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetPositionCommand : IEditorCommand, IMergeableCommand
    {
        public readonly IHasMutablePosition Target;

        public readonly Vector2 Position;

        public SetPositionCommand(IHasMutablePosition target, Vector2 position)
        {
            Target = target;
            Position = position;
        }

        public void Apply() => Target.Position = Position;

        public IEditorCommand CreateUndo() => new SetPositionCommand(Target, Target.Position);

        public bool IsRedundant => Position == Target.Position;

        public IEditorCommand? MergeWith(IEditorCommand previous)
        {
            if (previous is SetPositionCommand moveCommand)
                return moveCommand.Target != Target ? null : this;

            return null;
        }
    }
}
