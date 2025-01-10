// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        public readonly Bindable<bool> CanUndo = new BindableBool();

        public readonly Bindable<bool> CanRedo = new BindableBool();

        public bool HasUncommittedChanges => currentTransaction.UndoChanges.Count != 0;

        private Transaction currentTransaction;

        private readonly Stack<Transaction> undoStack = new Stack<Transaction>();

        private readonly Stack<Transaction> redoStack = new Stack<Transaction>();

        private bool isRestoring;

        public NewBeatmapEditorChangeHandler(EditorBeatmap editorBeatmap)
        {
            currentTransaction = new Transaction();
            this.editorBeatmap = editorBeatmap;

            editorBeatmap.TransactionBegan += BeginChange;
            editorBeatmap.TransactionEnded += EndChange;
            editorBeatmap.SaveStateTriggered += SaveState;
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

            revertTransaction(transaction);

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

            applyTransaction(transaction);

            undoStack.Push(transaction);

            historyChanged();

            return true;
        }

        private void revertTransaction(Transaction transaction)
        {
            isRestoring = true;
            editorBeatmap.BeginChange();

            foreach (var change in transaction.UndoChanges.Reverse())
                change.Revert();

            foreach (var hitObject in transaction.HitObjectUpdates)
                editorBeatmap.Update(hitObject);

            editorBeatmap.EndChange();
            isRestoring = false;
        }

        private void applyTransaction(Transaction transaction)
        {
            isRestoring = true;
            editorBeatmap.BeginChange();

            foreach (var change in transaction.UndoChanges)
                change.Apply();

            foreach (var hitObject in transaction.HitObjectUpdates)
                editorBeatmap.Update(hitObject);

            editorBeatmap.EndChange();
            isRestoring = false;
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
                undoChanges = new List<IRevertibleChange>();
            }

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
