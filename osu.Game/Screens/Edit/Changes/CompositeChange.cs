// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// This class provides a way to create more complex subroutines that consist of multiple <see cref="IRevertibleChange"/>s,
    /// while it itself can be used like any other <see cref="IRevertibleChange"/>.
    /// This is not meant to replace transactions, but rather for grouping multiple changes together within a transaction.
    /// </summary>
    public abstract class CompositeChange : IRevertibleChange
    {
        private List<IRevertibleChange>? changes;

        public void Apply()
        {
            if (changes == null)
            {
                changes = new List<IRevertibleChange>();
                ApplyChanges();
                return;
            }

            foreach (var change in changes)
                change.Apply();
        }

        public void Revert()
        {
            if (changes == null)
                throw new System.InvalidOperationException("Cannot revert before applying.");

            for (int i = changes.Count - 1; i >= 0; i--)
                changes[i].Revert();
        }

        /// <summary>
        /// Applies the given <see cref="IRevertibleChange"/> and records it in this <see cref="CompositeChange"/>.
        /// </summary>
        /// <param name="change">The <see cref="IRevertibleChange"/> to apply and record.</param>
        protected void Apply(IRevertibleChange change)
        {
            change.Apply();
            changes!.Add(change);
        }

        /// <summary>
        /// Applies and records the changes of this <see cref="CompositeChange"/>.
        /// </summary>
        /// <remarks>Use <see cref="Apply(IRevertibleChange)"/> to apply the <see cref="IRevertibleChange"/> created in this method.</remarks>
        protected abstract void ApplyChanges();
    }
}
