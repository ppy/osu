// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A component that tracks a batch change, only applying after all active changes are completed.
    /// </summary>
    public abstract class TransactionalCommitComponent
    {
        public bool TransactionActive => bulkChangesStarted > 0;

        private int bulkChangesStarted;

        /// <summary>
        /// Signal the beginning of a change.
        /// </summary>
        public void BeginChange() => bulkChangesStarted++;

        /// <summary>
        /// Signal the end of a change.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if <see cref="BeginChange"/> was not first called.</exception>
        public void EndChange()
        {
            if (bulkChangesStarted == 0)
                throw new InvalidOperationException($"Cannot call {nameof(EndChange)} without a previous call to {nameof(BeginChange)}.");

            if (--bulkChangesStarted == 0)
                UpdateState();
        }

        public void SaveState()
        {
            if (bulkChangesStarted > 0)
                return;

            UpdateState();
        }

        protected abstract void UpdateState();
    }
}
