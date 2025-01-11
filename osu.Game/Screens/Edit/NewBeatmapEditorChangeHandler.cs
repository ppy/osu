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
    public partial class NewBeatmapEditorChangeHandler : TransactionalCommitComponent
    {
        private readonly EditorBeatmap editorBeatmap;

        /// <summary>
        /// This change handler will be suppressed while a transaction with this command handler is in progress.
        /// Any save states of this change handler will be added to the undo stack.
        /// </summary>
        private readonly EditorChangeHandler? changeHandler;

        public readonly Bindable<bool> CanUndo = new BindableBool();

        public readonly Bindable<bool> CanRedo = new BindableBool();

        public event Action? OnStateChange;

        public bool HasUncommittedChanges => currentTransaction.UndoChanges.Count != 0;

        private Transaction currentTransaction;

        private readonly Stack<Transaction> undoStack = new Stack<Transaction>();

        private readonly Stack<Transaction> redoStack = new Stack<Transaction>();

        private bool isRestoring;

        public NewBeatmapEditorChangeHandler(EditorBeatmap editorBeatmap, EditorChangeHandler? changeHandler)
        {
            currentTransaction = new Transaction();
            this.editorBeatmap = editorBeatmap;
            this.changeHandler = changeHandler;

            editorBeatmap.TransactionBegan += BeginChange;
            editorBeatmap.TransactionEnded += EndChange;
            editorBeatmap.SaveStateTriggered += SaveState;

            if (this.changeHandler != null)
                this.changeHandler.OnStateChange += commitChangeHandlerStateChange;
        }

        /// <summary>
        /// Submits a change to be applied and added to the history.
        /// </summary>
        /// <param name="change">Change to be applied.</param>
        /// <param name="commitImmediately">Whether to commit the current transaction and push it onto the undo stack immediately.</param>
        public void Submit(IRevertibleChange change, bool commitImmediately = false)
        {
            if (commitImmediately)
                BeginChange();

            change.Apply();
            Record(change);

            if (commitImmediately)
                EndChange();
        }

        /// <summary>
        /// Submits a collection of changes to be applied and added to the history.
        /// </summary>
        /// <param name="changes">Changes to be applied.</param>
        /// <param name="commitImmediately">Whether to commit the current transaction and push it onto the undo stack immediately.</param>
        public void Submit(IEnumerable<IRevertibleChange> changes, bool commitImmediately = false)
        {
            foreach (var change in changes)
                Submit(change);

            if (commitImmediately)
                UpdateState();
        }

        protected override void UpdateState()
        {
            if (isRestoring)
                return;

            if (!HasUncommittedChanges)
            {
                Logger.Log("Nothing to commit");
                return;
            }

            undoStack.Push(currentTransaction);
            redoStack.Clear();

            Logger.Log($"Added {currentTransaction.UndoChanges.Count} change(s) to undo stack");

            currentTransaction = new Transaction();

            OnStateChange?.Invoke();
            historyChanged();
        }

        private void commitChangeHandlerStateChange()
        {
            if (isRestoring || changeHandler!.CurrentState <= 0)
                return;

            undoStack.Push(new Transaction(true));
            redoStack.Clear();

            Logger.Log("Added old change handler transaction to undo stack");

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

            if (transaction.IsChangeHandlerTransaction)
            {
                isRestoring = true;
                changeHandler!.RestoreState(-1);
                isRestoring = false;
                Logger.Log("Undo handled by old change handler");
            }
            else
            {
                revertTransaction(transaction);
                Logger.Log("Undo handled by new change handler");
            }

            redoStack.Push(transaction);

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
                isRestoring = true;
                changeHandler!.RestoreState(1);
                isRestoring = false;
                Logger.Log("Redo handled by old change handler");
            }
            else
            {
                applyTransaction(transaction);
                Logger.Log("Redo handled by new change handler");
            }

            undoStack.Push(transaction);

            historyChanged();

            return true;
        }

        private void revertTransaction(Transaction transaction)
        {
            // We are navigating history so we don't want to write a new state.
            if (changeHandler != null)
                changeHandler.SuppressStateChange = true;

            isRestoring = true;
            editorBeatmap.BeginChange();

            foreach (var change in transaction.UndoChanges.Reverse())
                change.Revert();

            foreach (var hitObject in transaction.HitObjectUpdates)
                editorBeatmap.Update(hitObject);

            editorBeatmap.EndChange();
            isRestoring = false;

            if (changeHandler != null)
                changeHandler.SuppressStateChange = false;
        }

        private void applyTransaction(Transaction transaction)
        {
            // We are navigating history so we don't want to write a new state.
            if (changeHandler != null)
                changeHandler.SuppressStateChange = true;

            isRestoring = true;
            editorBeatmap.BeginChange();

            foreach (var change in transaction.UndoChanges)
                change.Apply();

            foreach (var hitObject in transaction.HitObjectUpdates)
                editorBeatmap.Update(hitObject);

            editorBeatmap.EndChange();
            isRestoring = false;

            if (changeHandler != null)
                changeHandler.SuppressStateChange = false;
        }

        private void historyChanged()
        {
            CanUndo.Value = undoStack.Count > 0;
            CanRedo.Value = redoStack.Count > 0;
        }

        /// <summary>
        /// Adds a change to the history but does not apply it.
        /// </summary>
        /// <param name="change">Change to be recorded.</param>
        public void Record(IRevertibleChange change)
        {
            currentTransaction.Add(change);
        }

        public void RecordUpdate(HitObject hitObject)
        {
            currentTransaction.RecordUpdate(hitObject);
        }

        private readonly struct Transaction
        {
            public Transaction()
            {
                IsChangeHandlerTransaction = false;
                undoChanges = new List<IRevertibleChange>();
            }

            public Transaction(bool isChangeHandlerTransaction)
            {
                IsChangeHandlerTransaction = isChangeHandlerTransaction;
                undoChanges = new List<IRevertibleChange>();
            }

            public readonly bool IsChangeHandlerTransaction;

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
