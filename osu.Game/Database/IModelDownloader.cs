// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;
using System;

namespace osu.Game.Database
{
    public interface IModelDownloader<TModel> : IModelManager<TModel>
        where TModel : class
    {
        /// <summary>
        /// Fired when a <see cref="TModel"/> download begins.
        /// </summary>
        event Action<ArchiveDownloadModelRequest<TModel>> DownloadBegan;

        /// <summary>
        /// Fired when a <see cref="TModel"/> download is interrupted, either due to user cancellation or failure.
        /// </summary>
        event Action<ArchiveDownloadModelRequest<TModel>> DownloadFailed;

        bool IsAvailableLocally(TModel model);

        /// <summary>
        /// Downloads a <see cref="TModel"/>.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <returns>Whether downloading can happen.</returns>
        bool Download(TModel model);

        /// <summary>
        /// Downloads a <see cref="TModel"/> with optional parameters for the download request.
        /// This will post notifications tracking progress.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> to be downloaded.</param>
        /// <param name="extra">Optional parameters to be used for creating the download request.</param>
        /// <returns>Whether downloading can happen.</returns>
        bool Download(TModel model, params object[] extra);

        /// <summary>
        /// Checks whether a given <see cref="TModel"/> is available in the local store already.
        /// </summary>
        /// <param name="model">The <see cref="TModel"/> whose existence needs to be checked.</param>
        /// <returns>Whether the <see cref="TModel"/> exists locally.</returns>
        ArchiveDownloadModelRequest<TModel> GetExistingDownload(TModel model);
    }
}
