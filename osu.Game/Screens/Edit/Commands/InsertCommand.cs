// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Screens.Edit.Commands
{
    public class InsertCommand<T, T2> : IEditorCommand where T : IList<T2>
    {
        public readonly T Target;

        public readonly int InsertionIndex;

        public readonly T2 Item;

        public InsertCommand(T target, int insertionIndex, T2 item)
        {
            Target = target;
            InsertionIndex = insertionIndex;
            Item = item;
        }

        public void Apply() => Target.Insert(InsertionIndex, Item);

        public IEditorCommand CreateUndo() => new RemoveCommand<T, T2>(Target, InsertionIndex);
    }
}
