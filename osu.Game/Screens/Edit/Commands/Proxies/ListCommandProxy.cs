// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Commands.Proxies
{
    public readonly struct ListCommandProxy<TItem, TItemProxy> : ICommandProxy<IList<TItem>>, IList<TItemProxy> where TItemProxy : ICommandProxy<TItem>, new()
    {
        public ListCommandProxy(EditorCommandHandler? commandHandler, IList<TItem> target)
        {
            CommandHandler = commandHandler;
            Target = target;
        }

        public EditorCommandHandler? CommandHandler { get; init; }
        public IList<TItem> Target { get; init; }

        public void Submit(IEditorCommand command) => CommandHandler.SafeSubmit(command);

        public IEnumerator<TItemProxy> GetEnumerator()
        {
            var commandHandler = CommandHandler;
            return Target.Select(o => new TItemProxy { CommandHandler = commandHandler, Target = o }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TItemProxy item) => Add(item.Target);

        public void Add(TItem item) => Insert(Count, item);

        public void Clear()
        {
            while (Target.Count > 0)
                RemoveAt(Count - 1);
        }

        public bool Contains(TItemProxy item) => Target.Contains(item.Target);

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

            Submit(new ListCommands.Remove<TItem>(Target, item));
            return true;
        }

        public int Count => Target.Count;

        public bool IsReadOnly => Target.IsReadOnly;

        public int IndexOf(TItemProxy item) => IndexOf(item.Target);

        public int IndexOf(TItem item) => Target.IndexOf(item);

        public void Insert(int index, TItemProxy item) => Insert(index, item.Target);

        public void Insert(int index, TItem item) => Submit(new ListCommands.Insert<TItem>(Target, index, item));

        public void RemoveAt(int index) => Submit(new ListCommands.Remove<TItem>(Target, index));

        public void RemoveRange(int index, int count)
        {
            for (int i = 0; i < count; i++)
                RemoveAt(index);
        }

        public TItemProxy this[int index]
        {
            get => new TItemProxy { CommandHandler = CommandHandler, Target = Target[index] };
            set => Submit(new ListCommands.Update<TItem>(Target, index, value.Target));
        }
    }
}
