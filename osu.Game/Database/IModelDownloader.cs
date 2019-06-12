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

        bool IsAvailableLocally(TModel model);

        /// <summary>
        /// Downloads a <see cref="TModel"/>.
        /// This may post notifications tracking progress.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <returns>Whether downloading can happen.</returns>
        bool Download(TModel model);

        /// <summary>
        /// Downloads a <see cref="TModel"/> with optional parameters for the download request.
        /// This may post notifications tracking progress.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <param name="extra">Optional parameters to be used for creating the download request.</param>
        /// <returns>Whether downloading can happen.</returns>
        bool Download(TModel model, params object[] extra);

        /// <summary>
        /// Gets an existing <see cref="TModel"/> download request if it exists.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> whose request is wanted.</param>
        /// <returns>The <see cref="ArchiveDownloadRequest{TModel}"/> object if it exists, otherwise null.</returns>
        ArchiveDownloadRequest<TModel> GetExistingDownload(TModel model);
    }
}
