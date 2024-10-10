// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Commands
{
    public interface ICommandProxy<T>
    {
        EditorCommandHandler? CommandHandler { get; init; }
        T Target { get; init; }
        void Submit(IEditorCommand command);
    }

    public readonly struct CommandProxy<T> : ICommandProxy<T>
    {
        public CommandProxy(EditorCommandHandler? commandHandler, T target)
        {
            CommandHandler = commandHandler;
            Target = target;
        }

        public EditorCommandHandler? CommandHandler { get; init; }
        public T Target { get; init; }
        public void Submit(IEditorCommand command) => CommandHandler.SafeSubmit(command);
    }

    public readonly struct ListCommandProxy<T, TItemProxy, TItem> : ICommandProxy<T>, IList<TItemProxy> where T : IList<TItem> where TItemProxy : ICommandProxy<TItem>, new()
    {
        public ListCommandProxy(EditorCommandHandler? commandHandler, T target)
        {
            CommandHandler = commandHandler;
            Target = target;
        }

        public EditorCommandHandler? CommandHandler { get; init; }
        public T Target { get; init; }
        public void Submit(IEditorCommand command) => CommandHandler.SafeSubmit(command);

        public IEnumerator<TItemProxy> GetEnumerator()
        {
            var commandHandler = CommandHandler;
            return Target.Select(o => new TItemProxy { CommandHandler = commandHandler, Target = o }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TItemProxy item)
        {
            Add(item.Target);
        }

        public void Add(TItem item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            while (Target.Count > 0)
                RemoveAt(Count - 1);
        }

        public bool Contains(TItemProxy item)
        {
            return Target.Contains(item.Target);
        }

        public void CopyTo(TItemProxy[] array, int arrayIndex)
        {
            for (int i = 0; i < Target.Count; i++)
                array[arrayIndex + i] = new TItemProxy { CommandHandler = CommandHandler, Target = Target[i] };
        }

        public bool Remove(TItemProxy item) => Remove(item.Target);

        public bool Remove(TItem item)
        {
            if (!Target.Contains(item))
                return false;

            Submit(new RemoveCommand<T, TItem>(Target, item));
            return true;
        }

        public int Count => Target.Count;
        public bool IsReadOnly => Target.IsReadOnly;

        public int IndexOf(TItemProxy item)
        {
            return Target.IndexOf(item.Target);
        }

        public void Insert(int index, TItemProxy item)
        {
            Insert(index, item.Target);
        }

        public void Insert(int index, TItem item)
        {
            Submit(new InsertCommand<T, TItem>(Target, index, item));
        }

        public void RemoveAt(int index)
        {
            Submit(new RemoveCommand<T, TItem>(Target, index));
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = 0; i < count; i++)
                RemoveAt(index);
        }

        public TItemProxy this[int index]
        {
            get => new TItemProxy { CommandHandler = CommandHandler, Target = Target[index] };
            set => Submit(new InsertCommand<T, TItem>(Target, index, value.Target));
        }
    }
}
