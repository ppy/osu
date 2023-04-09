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

        public Action<Notification>? PostNotification { get; set; }

        // Store the model being exporting.
        private static readonly List<Live<TModel>> exporting_models = new List<Live<TModel>>();

        /// <summary>
        /// Construct exporter.
        /// Create a new exporter for each export, otherwise it will cause confusing notifications.
        /// </summary>
        /// <param name="storage">Storage for storing exported files. Basically it is used to provide export stream</param>
        protected LegacyModelExporter(Storage storage)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
        }

        /// <summary>
        /// Export the model to default folder.
        /// </summary>
        /// <param name="model">The model should export.</param>
        /// <param name="realm">Realm that convert model to Live.</param>
        /// <param name="cancellationToken">
        /// The Cancellation token that can cancel the exporting.
        /// If specified CancellationToken, then use it. Otherwise use PostNotification's CancellationToken.
        /// </param>
        /// <returns></returns>
        public Task<bool> ExportAsync(TModel model, RealmAccess realm, CancellationToken cancellationToken = default)
        {
            return ExportAsync(model.ToLive(realm), cancellationToken);
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
        public async Task<bool> ExportAsync(Live<TModel> model, CancellationToken cancellationToken = default)
        {
            // check if the model is being exporting already
            if (!exporting_models.Contains(model))
            {
                exporting_models.Add(model);
            }
            else
            {
                // model is being exported
                return false;
            }

            string itemFilename = model.PerformRead(s => GetFilename(s).GetValidFilename());

            IEnumerable<string> existingExports =
                exportStorage
                    .GetFiles(string.Empty, $"{itemFilename}*{FileExtension}")
                    .Concat(exportStorage.GetDirectories(string.Empty));
            string filename = NamingUtils.GetNextBestFilename(existingExports, $"{itemFilename}{FileExtension}");
            bool success = false;

            ProgressNotification notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            PostNotification?.Invoke(notification);

            try
            {
                using (var stream = exportStorage.CreateFileSafely(filename))
                {
                    success = await ExportToStreamAsync(model, stream, notification,
                        cancellationToken == CancellationToken.None ? notification.CancellationToken : cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                success = false;
            }

            // cleanup if export is failed or canceled.
            if (!success)
            {
                notification.State = ProgressNotificationState.Cancelled;
                exportStorage.Delete(filename);
            }
            else
            {
                notification.CompletionText = "Export Complete, Click to open the folder";
                notification.CompletionClickAction = () => exportStorage.PresentFileExternally(filename);
                notification.State = ProgressNotificationState.Completed;
            }

            exporting_models.Remove(model);

            return success;
        }

        /// <summary>
        /// Export model to stream.
        /// </summary>
        /// <param name="model">The model which have <see cref="IHasGuidPrimaryKey"/>.</param>
        /// <param name="stream">The stream to export.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        /// <param name="cancellationToken">The Cancellation token that can cancel the exporting.</param>
        /// <returns>Whether the export was successful</returns>
        public Task<bool> ExportToStreamAsync(Live<TModel> model, Stream stream, ProgressNotification? notification = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                model.PerformRead(s =>
                {
                    ExportToStream(s, stream, notification, cancellationToken);
                });
            }, cancellationToken).ContinueWith(t =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                if (t.IsFaulted)
                {
                    Logger.Error(t.Exception, "An error occurred while exporting", LoggingTarget.Database);
                    return false;
                }

                return true;
            }, CancellationToken.None);
        }

        /// <summary>
        /// Exports model to Stream.
        /// </summary>
        /// <param name="model">The model to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        /// <param name="cancellationToken">The Cancellation token that can cancel the exporting.</param>
        protected abstract void ExportToStream(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default);
    }
}
