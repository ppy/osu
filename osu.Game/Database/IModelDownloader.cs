// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;
using System;

namespace osu.Game.Database
{
    /// <summary>
    /// Represents a <see cref="IModelManager{TModel}"/> that can download new models from an external source.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelDownloader<TModel> : IModelManager<TModel>
        where TModel : class
    {
        /// <summary>
        /// Fired when a <see cref="TModel"/> download begins.
        /// </summary>
        event Action<ArchiveDownloadRequest<TModel>> DownloadBegan;

        /// <summary>
        /// Fired when a <see cref="TModel"/> download is interrupted, either due to user cancellation or failure.
        /// </summary>
        event Action<ArchiveDownloadRequest<TModel>> DownloadFailed;

        /// <summary>
        /// Checks whether a given <see cref="TModel"/> is already available in the local store.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> whose existence needs to be checked.</param>
        /// <returns>Whether the <see cref="TModel"/> exists.</returns>
        bool IsAvailableLocally(TModel model);

        /// <summary>
        /// Begin a download for the requested <see cref="TModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle..</param>
        /// <returns>Whether the download was started.</returns>
        bool Download(TModel model, bool minimiseDownloadSize);

        /// <summary>
        /// Gets an existing <see cref="TModel"/> download request if it exists.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> whose request is wanted.</param>
        /// <returns>The <see cref="ArchiveDownloadRequest{TModel}"/> object if it exists, otherwise null.</returns>
        ArchiveDownloadRequest<TModel> GetExistingDownload(TModel model);
    }
}
