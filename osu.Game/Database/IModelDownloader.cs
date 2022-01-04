// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API;

namespace osu.Game.Database
{
    /// <summary>
    /// Represents a <see cref="IModelManager{TModel}"/> that can download new models from an external source.
    /// </summary>
    /// <typeparam name="T">The item's interface type.</typeparam>
    public interface IModelDownloader<T> : IPostNotifications
        where T : class
    {
        /// <summary>
        /// Fired when a <typeparamref name="T"/> download begins.
        /// This is NOT run on the update thread and should be scheduled.
        /// </summary>
        event Action<ArchiveDownloadRequest<T>> DownloadBegan;

        /// <summary>
        /// Fired when a <typeparamref name="T"/> download is interrupted, either due to user cancellation or failure.
        /// This is NOT run on the update thread and should be scheduled.
        /// </summary>
        event Action<ArchiveDownloadRequest<T>> DownloadFailed;

        /// <summary>
        /// Begin a download for the requested <typeparamref name="T"/>.
        /// </summary>
        /// <param name="item">The <stypeparamref name="T"/> to be downloaded.</param>
        /// <param name="minimiseDownloadSize">Whether this download should be optimised for slow connections. Generally means extras are not included in the download bundle..</param>
        /// <returns>Whether the download was started.</returns>
        bool Download(T item, bool minimiseDownloadSize);

        /// <summary>
        /// Gets an existing <typeparamref name="T"/> download request if it exists.
        /// </summary>
        /// <param name="item">The <typeparamref name="T"/> whose request is wanted.</param>
        /// <returns>The <see cref="ArchiveDownloadRequest{T}"/> object if it exists, otherwise null.</returns>
        ArchiveDownloadRequest<T> GetExistingDownload(T item);
    }
}
