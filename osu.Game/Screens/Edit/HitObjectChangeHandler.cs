// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A change handler for handling hit object changes in an <see cref="EditorBeatmap"/> using the command design pattern.
    /// This is supposed to eventually replace the <see cref="EditorChangeHandler"/> and its inheritors once all the editor operations have been refactored.
    /// </summary>
    public partial class HitObjectChangeHandler : TransactionalCommitComponent, IBeatmapEditorChangeHandler
    {
        private readonly IBeatmapEditorChangeHandler? changeHandler;

        /// <summary>
        /// This change handler will be suppressed while a transaction with this command handler is in progress.
        /// Any save states of this change handler will be added to the undo stack.
        /// </summary>
        private readonly EditorChangeHandler? oldChangeHandler;

        public readonly Bindable<bool> CanUndo = new BindableBool();

        public readonly Bindable<bool> CanRedo = new BindableBool();

        public event Action? OnStateChange;

        public bool HasUncommittedChanges => currentTransaction.UndoChanges.Count != 0;

        public Guid CurrentState => undoStack.TryPeek(out var transaction) ? transaction.Id : Guid.Empty;

        private Transaction currentTransaction;

        private readonly Stack<Transaction> undoStack = new Stack<Transaction>();

        private readonly Stack<Transaction> redoStack = new Stack<Transaction>();

        private bool isRestoring;

        public HitObjectChangeHandler(IBeatmapEditorChangeHandler? changeHandler = null, EditorChangeHandler? oldChangeHandler = null)
        {
            currentTransaction = new Transaction();
            this.changeHandler = changeHandler;
            this.oldChangeHandler = oldChangeHandler;

            if (this.oldChangeHandler != null)
                this.oldChangeHandler.OnStateChange += commitOldChangeHandlerStateChange;
        }

        /// <summary>
        /// Adds a change to the history. The change should be applied before this method is called.
        /// </summary>
        /// <param name="change">Change to be recorded.</param>
        public void Record(IRevertibleChange change)
        {
            if (isRestoring)
                return;

            currentTransaction.Add(change);
        }

        /// <summary>
        /// Records to the history that a <see cref="HitObject"/> has been changed. This makes sure hit objects are properly updated on undo/redo operations.
        /// </summary>
        /// <param name="hitObject">Hit object which was changed.</param>
        public void Update(HitObject hitObject)
        {
            if (isRestoring)
                return;

            currentTransaction.RecordUpdate(hitObject);
        }

        protected override void UpdateState()
        {
            if (isRestoring || !HasUncommittedChanges)
                return;

            undoStack.Push(currentTransaction);
            redoStack.Clear();

            Logger.Log($"Added {currentTransaction.UndoChanges.Count} change(s) to undo stack");

            // There is always an ambient transaction ready to record changes,
            // so no explicit 'start transaction' is needed before changes can be recorded to the transaction.
            // This allows for transactions to be made via the SaveState() method.
            currentTransaction = new Transaction();

            OnStateChange?.Invoke();
            historyChanged();
        }

        /// <summary>
        /// This method will be removed once the <see cref="EditorChangeHandler"/> is fully replaced by this change handler.
        /// </summary>
        private void commitOldChangeHandlerStateChange()
        {
            if (isRestoring || oldChangeHandler!.CurrentState <= 0)
                return;

            undoStack.Push(new Transaction(true));
            redoStack.Clear();

            Logger.Log("Added old change handler transaction to undo stack");

            OnStateChange?.Invoke();
            historyChanged();
        }

        public void RestoreState(int direction)
        {
            switch (direction)
            {
                case 0:
                    return;

                case > 0:
                    Redo();
                    break;

                case < 0:
                    Undo();
                    break;
            }
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

            if (transaction.IsChangeHandlerTransaction)
            {
                // Handle undo via the old change handler for stuff like metadata and timing points
                // This will be removed once the old change handler is fully replaced by this change handler.
                isRestoring = true;
                oldChangeHandler!.RestoreState(-1);
                isRestoring = false;
                Logger.Log("Undo handled by old change handler");
            }
            else
            {
                revertTransaction(transaction);
                Logger.Log("Undo handled by new change handler");
            }

            redoStack.Push(transaction);

            OnStateChange?.Invoke();
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

            if (transaction.IsChangeHandlerTransaction)
            {
                // Handle redo via the old change handler for stuff like metadata and timing points
                // This will be removed once the old change handler is fully replaced by this change handler.
                isRestoring = true;
                oldChangeHandler!.RestoreState(1);
                isRestoring = false;
                Logger.Log("Redo handled by old change handler");
            }
            else
            {
                applyTransaction(transaction);
                Logger.Log("Redo handled by new change handler");
            }

            undoStack.Push(transaction);

            OnStateChange?.Invoke();
            historyChanged();

            return true;
        }

        private void revertTransaction(Transaction transaction)
        {
            // We are navigating history so we don't want to write a new state.
            if (oldChangeHandler != null)
                oldChangeHandler.SuppressStateChange = true;

            isRestoring = true;
            changeHandler?.BeginChange();

            foreach (var change in transaction.UndoChanges.Reverse())
                change.Revert();

            foreach (var hitObject in transaction.HitObjectUpdates)
                changeHandler?.Update(hitObject);

            changeHandler?.EndChange();
            isRestoring = false;

            if (oldChangeHandler != null)
                oldChangeHandler.SuppressStateChange = false;
        }

        private void applyTransaction(Transaction transaction)
        {
            // We are navigating history so we don't want to write a new state.
            if (oldChangeHandler != null)
                oldChangeHandler.SuppressStateChange = true;

            isRestoring = true;
            changeHandler?.BeginChange();

            foreach (var change in transaction.UndoChanges)
                change.Apply();

            foreach (var hitObject in transaction.HitObjectUpdates)
                changeHandler?.Update(hitObject);

            changeHandler?.EndChange();
            isRestoring = false;

            if (oldChangeHandler != null)
                oldChangeHandler.SuppressStateChange = false;
        }

        private void historyChanged()
        {
            CanUndo.Value = undoStack.Count > 0;
            CanRedo.Value = redoStack.Count > 0;
        }

        /// <summary>
        /// A transaction is a collection of revertible changes that represent a single undo step.
        /// </summary>
        private readonly struct Transaction
        {
            public Transaction()
            {
                Id = Guid.NewGuid();
                IsChangeHandlerTransaction = false;
                undoChanges = new List<IRevertibleChange>();
            }

            public Transaction(bool isChangeHandlerTransaction)
            {
                Id = Guid.NewGuid();
                IsChangeHandlerTransaction = isChangeHandlerTransaction;
                undoChanges = new List<IRevertibleChange>();
            }

            /// <summary>
            /// True if this transaction was created by the <see cref="EditorChangeHandler"/> and should be handled by it.
            /// This will be removed once the <see cref="EditorChangeHandler"/> is fully replaced by this change handler.
            /// </summary>
            public readonly bool IsChangeHandlerTransaction;

            public readonly Guid Id;

            private readonly List<IRevertibleChange> undoChanges;

            private readonly HashSet<HitObject> hitObjectUpdates = new HashSet<HitObject>();

            /// <summary>
            /// The changes to undo the given transaction.
            /// Stored in reverse order of original changes to match execution order when undoing.
            /// </summary>
            public IReadOnlyList<IRevertibleChange> UndoChanges => undoChanges;

            public IReadOnlySet<HitObject> HitObjectUpdates => hitObjectUpdates;

            public void Add(IRevertibleChange change)
            {
                undoChanges.Add(change);
            }

            public void RecordUpdate(HitObject hitObject)
            {
                hitObjectUpdates.Add(hitObject);
            }
        }
    }
}
