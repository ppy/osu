// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles exporting legacy user data of a single type from osu-stable.
    /// </summary>
    public abstract class LegacyModelExporter<TModel>
        where TModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The file extension for exports (including the leading '.').
        /// </summary>
        protected abstract string FileExtension { get; }

        protected Storage UserFileStorage;
        private readonly Storage exportStorage;
        protected virtual string GetFilename(TModel item) => item.GetDisplayString();

        private readonly RealmAccess realmAccess;

        public Action<Notification>? PostNotification { get; set; }

        /// <summary>
        /// Construct exporter.
        /// Create a new exporter for each export, otherwise it will cause confusing notifications.
        /// </summary>
        /// <param name="storage">Storage for storing exported files. Basically it is used to provide export stream</param>
        /// <param name="realm">The RealmAccess used to provide the exported file.</param>
        protected LegacyModelExporter(Storage storage, RealmAccess realm)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
            realmAccess = realm;
        }

        /// <summary>
        /// Export the model to default folder.
        /// </summary>
        /// <param name="model">The model should export.</param>
        /// <param name="cancellationToken">
        /// The Cancellation token that can cancel the exporting.
        /// If specified CancellationToken, then use it. Otherwise use PostNotification's CancellationToken.
        /// </param>
        /// <returns></returns>
        public async Task ExportAsync(TModel model, CancellationToken? cancellationToken = null)
        {
            string itemFilename = GetFilename(model).GetValidFilename();
            IEnumerable<string> existingExports =
                exportStorage
                    .GetFiles(string.Empty, $"{itemFilename}*{FileExtension}")
                    .Concat(exportStorage.GetDirectories(string.Empty));
            string filename = NamingUtils.GetNextBestFilename(existingExports, $"{itemFilename}{FileExtension}");
            bool success;

            ProgressNotification notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            notification.CompletionClickAction += () => exportStorage.PresentFileExternally(filename);
            PostNotification?.Invoke(notification);

            try
            {
                using (var stream = exportStorage.CreateFileSafely(filename))
                {
                    success = await ExportToStreamAsync(model, stream, notification, cancellationToken ?? notification.CancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                success = false;
            }

            if (!success)
            {
                exportStorage.Delete(filename);
            }
        }

        /// <summary>
        /// Export model to stream.
        /// </summary>
        /// <param name="model">The medel which have <see cref="IHasGuidPrimaryKey"/>.</param>
        /// <param name="stream">The stream to export.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        /// <param name="cancellationToken">The Cancellation token that can cancel the exporting.</param>
        /// <returns>Whether the export was successful</returns>
        public async Task<bool> ExportToStreamAsync(TModel model, Stream stream, ProgressNotification? notification = null, CancellationToken cancellationToken = default)
        {
            ProgressNotification notify = notification ?? new ProgressNotification();

            Guid id = model.ID;
            return await Task.Run(() =>
            {
                realmAccess.Run(r =>
                {
                    TModel refetchModel = r.Find<TModel>(id);
                    ExportToStream(refetchModel, stream, notify, cancellationToken);
                });
            }, cancellationToken).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    notify.State = ProgressNotificationState.Cancelled;
                    Logger.Error(t.Exception, "An error occurred while exporting");
                    return false;
                }

                notify.CompletionText = "Export Complete, Click to open the folder";
                notify.State = ProgressNotificationState.Completed;
                return true;
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Exports an item to Stream.
        /// Override if custom export method is required.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        /// <param name="cancellationToken">The Cancellation token that can cancel the exporting.</param>
        protected abstract void ExportToStream(TModel model, Stream outputStream, ProgressNotification notification, CancellationToken cancellationToken = default);
    }
}
