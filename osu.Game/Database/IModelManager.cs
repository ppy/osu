// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    /// <summary>
    /// Represents a model manager that publishes events when <typeparamref name="TModel"/>s are added or removed.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelManager<TModel> : IPostNotifications
        where TModel : class
    {
        /// <summary>
        /// A bindable which contains a weak reference to the last item that was updated.
        /// This is not thread-safe and should be scheduled locally if consumed from a drawable component.
        /// </summary>
        IBindable<WeakReference<TModel>> ItemUpdated { get; }

        /// <summary>
        /// A bindable which contains a weak reference to the last item that was removed.
        /// This is not thread-safe and should be scheduled locally if consumed from a drawable component.
        /// </summary>
        IBindable<WeakReference<TModel>> ItemRemoved { get; }

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
        /// Import one or more <typeparamref name="TModel"/> items from filesystem <paramref name="paths"/>.
        /// </summary>
        /// <remarks>
        /// This will be treated as a low priority import if more than one path is specified; use <see cref="ArchiveModelManager{TModel,TFileModel}.Import(osu.Game.Database.ImportTask[])"/> to always import at standard priority.
        /// This will post notifications tracking progress.
        /// </remarks>
        /// <param name="paths">One or more archive locations on disk.</param>
        Task Import(params string[] paths);

        Task Import(params ImportTask[] tasks);

        Task<IEnumerable<TModel>> Import(ProgressNotification notification, params ImportTask[] tasks);

        /// <summary>
        /// Import one <typeparamref name="TModel"/> from the filesystem and delete the file on success.
        /// Note that this bypasses the UI flow and should only be used for special cases or testing.
        /// </summary>
        /// <param name="task">The <see cref="ImportTask"/> containing data about the <typeparamref name="TModel"/> to import.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The imported model, if successful.</returns>
        Task<TModel> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Silently import an item from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The archive to be imported.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        Task<TModel> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Silently import an item from a <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="item">The model to be imported.</param>
        /// <param name="archive">An optional archive to use for model population.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        Task<TModel> Import(TModel item, ArchiveReader archive = null, bool lowPriority = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a given <typeparamref name="TModel"/> is already available in the local store.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> whose existence needs to be checked.</param>
        /// <returns>Whether the <typeparamref name="TModel"/> exists.</returns>
        bool IsAvailableLocally(TModel model);

        /// <summary>
        /// A user displayable name for the model type associated with this manager.
        /// </summary>
        string HumanisedModelName => $"{typeof(TModel).Name.Replace(@"Info", "").ToLower()}";
    }
}
