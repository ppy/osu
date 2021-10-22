// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API;
using osu.Framework.Bindables;

namespace osu.Game.Database
{
    /// <summary>
    /// Represents a <see cref="IModelManager{TModel}"/> that can download new models from an external source.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelDownloader<TModel> : IPostNotifications
        where TModel : class, IHasOnlineID
    {
        /// <summary>
        /// Fired when a <typeparamref name="TModel"/> download begins.
        /// This is NOT run on the update thread and should be scheduled.
        /// </summary>
        IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> DownloadBegan { get; }

        /// <summary>
        /// Fired when a <typeparamref name="TModel"/> download is interrupted, either due to user cancellation or failure.
        /// This is NOT run on the update thread and should be scheduled.
        /// </summary>
        IBindable<WeakReference<ArchiveDownloadRequest<TModel>>> DownloadFailed { get; }

        /// <summary>
        /// Begin a download for the requested <typeparamref name="TModel"/>.
        /// </summary>
        /// <param name="model">The <stypeparamref name="TModel"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle..</param>
        /// <returns>Whether the download was started.</returns>
        bool Download(TModel model, bool minimiseDownloadSize);

        /// <summary>
        /// Gets an existing <typeparamref name="TModel"/> download request if it exists.
        /// </summary>
        /// <param name="model">The <typeparamref name="TModel"/> whose request is wanted.</param>
        /// <returns>The <see cref="ArchiveDownloadRequest{TModel}"/> object if it exists, otherwise null.</returns>
        ArchiveDownloadRequest<TModel> GetExistingDownload(IHasOnlineID model);
    }
}
