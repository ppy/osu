// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Screens.Edit.Changes
{
    public abstract class CompositeChange : IRevertibleChange
    {
        private List<IRevertibleChange>? changes;

        public void Apply()
        {
            if (changes == null)
            {
                changes = new List<IRevertibleChange>();
                SubmitChanges();
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

        protected void Submit(IRevertibleChange change)
        {
            change.Apply();
            changes!.Add(change);
        }

        /// <summary>
        /// Applies the tracks the changes of this <see cref="CompositeChange"/>.
        /// </summary>
        /// <remarks>Use <see cref="Submit"/> to apply the <see cref="IRevertibleChange"/> created in this method.</remarks>
        protected abstract void SubmitChanges();
    }
}
