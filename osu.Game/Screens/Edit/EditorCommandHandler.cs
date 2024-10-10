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
        public event Action<IEditorCommand>? CommandApplied;

        public readonly Bindable<bool> CanUndo = new BindableBool();

        public readonly Bindable<bool> CanRedo = new BindableBool();

        public bool HasUncommittedChanges => currentTransaction.Commands.Count != 0;

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

            Logger.Log($"Added {currentTransaction.Commands.Count} command(s) to undo stack");

            currentTransaction = new Transaction();

            historyChanged();

            return true;
        }

        public bool Undo()
        {
            if (undoStack.Count == 0)
                return false;

            var transaction = undoStack.Pop();
            var redoTransaction = transaction.Reverse();

            revertTransaction(transaction);

            redoStack.Push(redoTransaction);

            historyChanged();

            return true;
        }

        public bool Redo()
        {
            if (redoStack.Count == 0)
                return false;

            var transaction = redoStack.Pop();
            var undoTransaction = transaction.Reverse();

            foreach (var command in transaction.Commands)
                apply(command);

            undoStack.Push(undoTransaction);

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
            foreach (var command in transaction.Commands.Reverse())
                apply(command);
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

            currentTransaction.Add(reverse);
        }

        private readonly struct Transaction
        {
            public Transaction()
            {
            }

            private readonly List<IEditorCommand> commands = new List<IEditorCommand>();

            public IReadOnlyList<IEditorCommand> Commands => commands;

            public void Add(IEditorCommand command)
            {
                if (command is IMergeableCommand mergeable)
                {
                    for (int i = 0; i < commands.Count; i++)
                    {
                        var merged = mergeable.MergeWith(commands[i]);

                        if (merged == null)
                            continue;

                        command = merged;
                        commands.RemoveAt(i--);

                        if (command is IMergeableCommand newMergeable)
                        {
                            mergeable = newMergeable;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                commands.Add(command);
            }

            public Transaction Reverse()
            {
                var reversed = new Transaction();

                foreach (var command in Commands.Reverse())
                    reversed.Add(command.CreateUndo());

                return reversed;
            }
        }
    }
}
