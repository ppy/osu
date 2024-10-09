// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Screens.Edit
{
    public partial class EditorCommandHandler
    {
        public EditorCommandHandler()
        {
        }

        public event Action<IEditorCommand>? CommandApplied;

        public readonly Bindable<bool> CanUndo = new BindableBool();

        public readonly Bindable<bool> CanRedo = new BindableBool();

        public bool HasUncommittedChanges => currentTransaction.Entries.Count != 0;

        public void Submit(IEditorCommand command, bool commitImmediately = false)
        {
            if (command.IsRedundant)
                return;

            record(command);
            apply(command);

            if (commitImmediately)
                Commit();
        }

        public void Submit(IEnumerable<IEditorCommand> commands, bool commitImmediately = false)
        {
            foreach (var command in commands)
                Submit(command);

            if (commitImmediately)
                Commit();
        }

        public bool Commit()
        {
            if (!HasUncommittedChanges)
            {
                Logger.Log("Nothing to commit");
                return false;
            }

            undoStack.Push(currentTransaction);
            redoStack.Clear();

            Logger.Log($"Added {currentTransaction.Entries.Count} command(s) to undo stack");

            currentTransaction = new Transaction();

            historyChanged();

            return true;
        }

        public bool Undo()
        {
            if (undoStack.Count == 0)
                return false;

            var transaction = undoStack.Pop();

            revertTransaction(transaction);

            redoStack.Push(transaction);

            historyChanged();

            return true;
        }

        public bool Redo()
        {
            if (redoStack.Count == 0)
                return false;

            var transaction = redoStack.Pop();

            foreach (var entry in transaction.Entries)
                apply(entry.Command);

            undoStack.Push(transaction);

            historyChanged();

            return true;
        }

        public bool RevertUncommitedChanges()
        {
            if (!HasUncommittedChanges)
                return false;

            revertTransaction(currentTransaction);

            currentTransaction = new Transaction();

            return true;
        }

        private void revertTransaction(Transaction transaction)
        {
            foreach (var entry in transaction.Entries.Reverse())
                apply(entry.Reverse);
        }

        private void historyChanged()
        {
            CanUndo.Value = undoStack.Count > 0;
            CanRedo.Value = redoStack.Count > 0;
        }

        private Transaction currentTransaction = new Transaction();

        private readonly Stack<Transaction> undoStack = new Stack<Transaction>();

        private readonly Stack<Transaction> redoStack = new Stack<Transaction>();

        private void apply(IEditorCommand command)
        {
            command.Apply();
            CommandApplied?.Invoke(command);
        }

        private void record(IEditorCommand command)
        {
            var reverse = command.CreateUndo();

            currentTransaction.Add(new HistoryEntry(command, reverse));
        }

        private readonly record struct HistoryEntry(IEditorCommand Command, IEditorCommand Reverse);

        private readonly struct Transaction
        {
            public Transaction()
            {
            }

            private readonly List<HistoryEntry> entries = new List<HistoryEntry>();

            public IReadOnlyList<HistoryEntry> Entries => entries;

            public void Add(HistoryEntry entry) => entries.Add(entry);
        }
    }
}
