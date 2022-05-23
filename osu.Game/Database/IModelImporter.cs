// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.IO.Archives;
using osu.Game.Overlays.Notifications;

#nullable enable

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles importing of associated models to the game store.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelImporter<TModel> : IPostNotifications, IPostImports<TModel>, ICanAcceptFiles
        where TModel : class, IHasGuidPrimaryKey
    {
        Task<IEnumerable<Live<TModel>>> Import(ProgressNotification notification, params ImportTask[] tasks);

        /// <summary>
        /// Import one <typeparamref name="TModel"/> from the filesystem and delete the file on success.
        /// Note that this bypasses the UI flow and should only be used for special cases or testing.
        /// </summary>
        /// <param name="task">The <see cref="ImportTask"/> containing data about the <typeparamref name="TModel"/> to import.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The imported model, if successful.</returns>
        Task<Live<TModel>?> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Silently import an item from an <see cref="ArchiveReader"/>.
        /// </summary>
        /// <param name="archive">The archive to be imported.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        Task<Live<TModel>?> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Silently import an item from a <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="item">The model to be imported.</param>
        /// <param name="archive">An optional archive to use for model population.</param>
        /// <param name="lowPriority">Whether this is a low priority import.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        Live<TModel>? Import(TModel item, ArchiveReader? archive = null, bool lowPriority = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// A user displayable name for the model type associated with this manager.
        /// </summary>
        string HumanisedModelName => $"{typeof(TModel).Name.Replace(@"Info", "").ToLower()}";
    }
}
