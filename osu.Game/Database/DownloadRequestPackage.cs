// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;

namespace osu.Game.Database
{
    /// <summary>
    /// A package that provides a download request and its corresponding progress notification.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public struct DownloadRequestPackage<TModel>
        where TModel : class
    {
        /// <summary>
        /// The download request of this <see cref="DownloadRequestPackage{TModel}"/>.
        /// </summary>
        public ArchiveDownloadRequest<TModel> Request;

        /// <summary>
        /// The download notification corresponding to this <see cref="Request"/>.
        /// </summary>
        public DownloadNotification Notification;
    }
}
