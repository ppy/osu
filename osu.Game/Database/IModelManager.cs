// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Database
{
    /// <summary>
    /// Represents a model manager that publishes events when <typeparamref name="TModel"/>s are added or removed.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelManager<TModel>
        where TModel : class
    {
        /// <summary>
        /// Delete an item from the manager.
        /// Is a no-op for already deleted items.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>false if no operation was performed</returns>
        bool Delete(TModel item);

        /// <summary>
        /// Delete multiple items.
        /// This will post notifications tracking progress.
        /// </summary>
        void Delete(List<TModel> items, bool silent = false);

        /// <summary>
        /// Restore multiple items that were previously deleted.
        /// This will post notifications tracking progress.
        /// </summary>
        void Undelete(List<TModel> items, bool silent = false);

        /// <summary>
        /// Restore an item that was previously deleted. Is a no-op if the item is not in a deleted state, or has its protected flag set.
        /// </summary>
        /// <param name="item">The item to restore</param>
        void Undelete(TModel item);

        /// <summary>
        /// Checks whether a given <typeparamref name="TModel"/> is already available in the local store.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> whose existence needs to be checked.</param>
        /// <returns>Whether the <typeparamref name="TModel"/> exists.</returns>
        bool IsAvailableLocally(TModel model);
    }
}
