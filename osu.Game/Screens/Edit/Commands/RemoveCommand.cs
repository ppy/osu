// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Screens.Edit.Commands
{
    public class RemoveCommand<T, T2> : IEditorCommand where T : IList<T2>
    {
        public readonly T Target;

        public readonly int Index;

        public RemoveCommand(T target, int index)
        {
            Target = target;
            Index = index;
        }

        public RemoveCommand(T target, T2 item)
        {
            Target = target;
            Index = target.IndexOf(item);
        }

        public void Apply() => Target.RemoveAt(Index);

        public IEditorCommand CreateUndo() => new InsertCommand<T, T2>(Target, Index, Target[Index]);
    }
}
