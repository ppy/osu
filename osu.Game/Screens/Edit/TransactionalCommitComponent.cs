// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A component that tracks a batch change, only applying after all active changes are completed.
    /// </summary>
    public abstract partial class TransactionalCommitComponent : Component
    {
        /// <summary>
        /// Fires whenever a transaction begins. Will not fire on nested transactions.
        /// </summary>
        public event Action? TransactionBegan;

        /// <summary>
        /// Fires when the last transaction completes.
        /// </summary>
        public event Action? TransactionEnded;

        /// <summary>
        /// Fires when <see cref="SaveState"/> is called and results in a non-transactional state save.
        /// </summary>
        public event Action? SaveStateTriggered;

        public bool TransactionActive => bulkChangesStarted > 0;

        private int bulkChangesStarted;

        /// <summary>
        /// Signal the beginning of a change.
        /// </summary>
        public virtual void BeginChange()
        {
            if (bulkChangesStarted++ == 0)
                TransactionBegan?.Invoke();
        }

        /// <summary>
        /// Signal the end of a change.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if <see cref="BeginChange"/> was not first called.</exception>
        public void EndChange()
        {
            if (bulkChangesStarted == 0)
                throw new InvalidOperationException($"Cannot call {nameof(EndChange)} without a previous call to {nameof(BeginChange)}.");

            if (--bulkChangesStarted == 0)
            {
                UpdateState();
                TransactionEnded?.Invoke();
            }
        }

        /// <summary>
        /// Force an update of the state with no attached transaction.
        /// This is a no-op if a transaction is already active. Should generally be used as a safety measure to ensure granular changes are not left outside a transaction.
        /// </summary>
        public void SaveState()
        {
            if (bulkChangesStarted > 0)
                return;

            SaveStateTriggered?.Invoke();
            UpdateState();
        }

        protected abstract void UpdateState();
    }
}
