// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using osu.Game.IO;

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
        /// Fired when an item is updated.
        /// </summary>
        event Action<TModel> ItemUpdated;

        /// <summary>
        /// Fired when an item is removed.
        /// </summary>
        event Action<TModel> ItemRemoved;

        /// <summary>
        /// This is a temporary method and will likely be replaced by a full-fledged (and more correctly placed) migration process in the future.
        /// </summary>
        Task ImportFromStableAsync(StableStorage stableStorage);

        /// <summary>
        /// Exports an item to a legacy (.zip based) package.
        /// </summary>
        /// <param name="item">The item to export.</param>
        void Export(TModel item);

        /// <summary>
        /// Exports an item to the given output stream.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        void ExportModelTo(TModel model, Stream outputStream);

        /// <summary>
        /// Perform an update of the specified item.
        /// TODO: Support file additions/removals.
        /// </summary>
        /// <param name="item">The item to update.</param>
        void Update(TModel item);

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
