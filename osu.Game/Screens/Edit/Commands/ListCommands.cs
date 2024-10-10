// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Screens.Edit.Commands
{
    public static class ListCommands
    {
        public class Update<T> : PropertyChangeCommand<IList<T>, T>
        {
            public int Index;

            public Update(IList<T> target, int index, T value)
                : base(target, value)
            {
                Index = index;
            }

            protected override T ReadValue(IList<T> target) => target[Index];

            protected override void WriteValue(IList<T> target, T value) => target[Index] = value;

            protected override Update<T> CreateInstance(IList<T> target, T value) => new Update<T>(target, Index, value);
        }

        public class Insert<T> : IEditorCommand
        {
            public readonly IList<T> Target;

            public readonly int InsertionIndex;

            public readonly T Item;

            public Insert(IList<T> target, int insertionIndex, T item)
            {
                Target = target;
                InsertionIndex = insertionIndex;
                Item = item;
            }

            public void Apply() => Target.Insert(InsertionIndex, Item);

            public IEditorCommand CreateUndo() => new Remove<T>(Target, InsertionIndex);
        }

        public class Remove<T> : IEditorCommand
        {
            public readonly IList<T> Target;

            public readonly int Index;

            public Remove(IList<T> target, int index)
            {
                Target = target;
                Index = index;
            }

            public Remove(IList<T> target, T item)
            {
                Target = target;
                Index = target.IndexOf(item);
            }

            public void Apply() => Target.RemoveAt(Index);

            public bool IsRedundant => Index < 0 || Index >= Target.Count;

            public IEditorCommand CreateUndo() => new Insert<T>(Target, Index, Target[Index]);
        }
    }
}
