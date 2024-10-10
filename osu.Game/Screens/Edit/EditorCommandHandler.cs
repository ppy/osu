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

        public bool HasUncommittedChanges => currentTransaction.UndoCommands.Count != 0;

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

            Logger.Log($"Added {currentTransaction.UndoCommands.Count} command(s) to undo stack");

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

            revertTransaction(transaction);

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
            foreach (var command in transaction.UndoCommands)
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
                this.undoCommands = new List<IEditorCommand>();
            }

            private Transaction(List<IEditorCommand> undoCommands)
            {
                this.undoCommands = undoCommands;
            }

            private readonly List<IEditorCommand> undoCommands;

            /// <summary>
            /// The commands to undo the given transaction.
            /// Stored in reverse order of original commands to match execution order when undoing.
            /// </summary>
            public IReadOnlyList<IEditorCommand> UndoCommands => undoCommands;

            public void Add(IEditorCommand command)
            {
                for (int i = 0; i < undoCommands.Count; i++)
                {
                    var other = undoCommands[i];

                    // Since the commands are stored in reverse order (to match execution order when undoing), the
                    // command we're inserting is treated as the previous command relative to the current one.
                    if (other is IMergeableCommand mergeable && mergeable.MergeWith(command, out var merged))
                    {
                        undoCommands[i] = merged;

                        // Since currently there's only one command that a given command can be merged with, we can
                        // stop iterating through the list once we've found a match.
                        return;
                    }
                }

                undoCommands.Insert(0, command);
            }

            public Transaction Reverse()
            {
                var commands = UndoCommands.Reverse().Select(command => command.CreateUndo()).ToList();

                return new Transaction(commands);
            }
        }
    }
}
