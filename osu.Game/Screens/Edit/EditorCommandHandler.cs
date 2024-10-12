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
    public partial class EditorCommandHandler : TransactionalCommitComponent
    {
        private readonly EditorBeatmap editorBeatmap;

        /// <summary>
        /// This change handler will be suppressed while a transaction with this command handler is in progress.
        /// Any save states of this change handler will be added to the undo stack.
        /// </summary>
        private readonly EditorChangeHandler? changeHandler;

        public event Action<IEditorCommand>? CommandApplied;

        public readonly Bindable<bool> CanUndo = new BindableBool();

        public readonly Bindable<bool> CanRedo = new BindableBool();

        public bool HasUncommittedChanges => currentTransaction.UndoCommands.Count != 0;

        private bool ignoreCommandStateChange;
        private bool ignoreChangeHandlerStateChange;

        private Transaction currentTransaction;

        private readonly Stack<Transaction> undoStack = new Stack<Transaction>();

        private readonly Stack<Transaction> redoStack = new Stack<Transaction>();

        public EditorCommandHandler(EditorBeatmap editorBeatmap, EditorChangeHandler? changeHandler)
        {
            this.editorBeatmap = editorBeatmap;
            this.changeHandler = changeHandler;
            currentTransaction = new Transaction();

            if (this.changeHandler != null)
            {
                TransactionBegan += () =>
                {
                    ignoreChangeHandlerStateChange = !ignoreCommandStateChange;
                    // The change handler should save states even when a change is recorded as commands,
                    // because it needs to know the state after the commands in order to restore it.
                    this.changeHandler.BeginChange();
                };
                TransactionEnded += () =>
                {
                    this.changeHandler.EndChange();
                    ignoreChangeHandlerStateChange = false;
                };
                this.changeHandler.TransactionBegan += () => ignoreCommandStateChange = !ignoreChangeHandlerStateChange;
                this.changeHandler.TransactionEnded += () => ignoreCommandStateChange = false;
                this.changeHandler.OnStateChange += commitChangeHandlerStateChange;
            }
        }

        /// <summary>
        /// Submits a command to be applied and added to the history.
        /// </summary>
        /// <param name="command">Command to be applied.</param>
        /// <param name="commitImmediately">Whether to commit the current transaction and push it onto the undo stack immediately.</param>
        public void Submit(IEditorCommand command, bool commitImmediately = false)
        {
            if (command.IsRedundant)
                return;

            record(command);
            apply(command);

            if (commitImmediately)
                UpdateState();
        }

        /// <summary>
        /// Submits a collection of commands to be applied and added to the history.
        /// </summary>
        /// <param name="commands">Commands to be applied.</param>
        /// <param name="commitImmediately">Whether to commit the current transaction and push it onto the undo stack immediately.</param>
        public void Submit(IEnumerable<IEditorCommand> commands, bool commitImmediately = false)
        {
            foreach (var command in commands)
                Submit(command);

            if (commitImmediately)
                UpdateState();
        }

        protected override void UpdateState()
        {
            if (!HasUncommittedChanges)
            {
                Logger.Log("Nothing to commit");
                return;
            }

            if (!ignoreCommandStateChange)
            {
                undoStack.Push(currentTransaction);
                redoStack.Clear();

                Logger.Log($"Added {currentTransaction.UndoCommands.Count} command(s) to undo stack");
            }

            currentTransaction = new Transaction();

            historyChanged();
        }

        private void commitChangeHandlerStateChange()
        {
            if (ignoreChangeHandlerStateChange || changeHandler!.CurrentState <= 0)
                return;

            undoStack.Push(new Transaction(isChangeHandlerTransaction: true));
            redoStack.Clear();

            Logger.Log("Added change handler transaction to undo stack");

            historyChanged();
        }

        /// <summary>
        /// Undoes the last transaction from the undo stack.
        /// Returns false if there are is nothing to undo.
        /// </summary>
        public bool Undo()
        {
            if (undoStack.Count == 0)
                return false;

            var transaction = undoStack.Pop();
            var redoTransaction = transaction.Reverse();

            if (transaction.IsChangeHandlerTransaction)
            {
                ignoreChangeHandlerStateChange = true;
                changeHandler!.RestoreState(-1);
                ignoreChangeHandlerStateChange = false;
                Logger.Log("Undo handled by change handler");
            }
            else
            {
                revertTransaction(transaction);

                if (changeHandler != null)
                    changeHandler.CurrentState--;
                Logger.Log("Undo handled by command handler");
            }

            redoStack.Push(redoTransaction);

            historyChanged();

            return true;
        }

        /// <summary>
        /// Redoes the last transaction from the redo stack.
        /// Returns false if there are is nothing to redo.
        /// </summary>
        public bool Redo()
        {
            if (redoStack.Count == 0)
                return false;

            var transaction = redoStack.Pop();
            var undoTransaction = transaction.Reverse();

            if (transaction.IsChangeHandlerTransaction)
            {
                ignoreChangeHandlerStateChange = true;
                changeHandler!.RestoreState(1);
                ignoreChangeHandlerStateChange = false;
                Logger.Log("Redo handled by change handler");
            }
            else
            {
                revertTransaction(transaction);

                if (changeHandler != null)
                    changeHandler.CurrentState++;
                Logger.Log("Redo handled by command handler");
            }

            undoStack.Push(undoTransaction);

            historyChanged();

            return true;
        }

        /// <summary>
        /// Reverts any changes that have been made since the last commit.
        /// Returns false if there are no uncommitted changes.
        /// </summary>
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
            // We are navigating history so we don't want to write a new state.
            if (changeHandler != null)
                changeHandler.SuppressStateChange = true;

            editorBeatmap.BeginChange();

            foreach (var command in transaction.UndoCommands)
                apply(command);

            editorBeatmap.EndChange();

            if (changeHandler != null)
                changeHandler.SuppressStateChange = false;
        }

        private void historyChanged()
        {
            CanUndo.Value = undoStack.Count > 0;
            CanRedo.Value = redoStack.Count > 0;
        }

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
            public readonly bool IsChangeHandlerTransaction;

            public Transaction()
            {
                undoCommands = new List<IEditorCommand>();
            }

            public Transaction(bool isChangeHandlerTransaction = false)
            {
                IsChangeHandlerTransaction = isChangeHandlerTransaction;
                undoCommands = new List<IEditorCommand>();
            }

            private Transaction(List<IEditorCommand> undoCommands, bool isChangeHandlerTransaction = false)
            {
                IsChangeHandlerTransaction = isChangeHandlerTransaction;
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
                if (command is IMergeableCommand mergeable)
                {
                    for (int i = 0; i < undoCommands.Count; i++)
                    {
                        // Since the commands are stored in reverse order (to match execution order when undoing), the
                        // command we're inserting is treated as the previous command relative to the current one.
                        if (mergeable.MergeWithNext(nextCommand: undoCommands[i], out var merged))
                        {
                            undoCommands[i] = merged;

                            // Since currently there's only one command that a given command can be merged with, we can
                            // stop iterating through the list once we've found a match.
                            return;
                        }
                    }
                }

                undoCommands.Insert(0, command);
            }

            public Transaction Reverse()
            {
                var commands = UndoCommands.Reverse().Select(command => command.CreateUndo()).ToList();

                return new Transaction(commands, IsChangeHandlerTransaction);
            }
        }
    }
}
