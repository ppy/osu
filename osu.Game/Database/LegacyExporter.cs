// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public abstract class LegacyExporter<TModel>
        where TModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
    {
        /// <summary>
        /// Max length of filename (including extension).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The filename limit for most OSs is 255. This actual usable length is smaller because <see cref="Storage.CreateFileSafely(string)"/> adds an additional "_<see cref="Guid"/>" to the end of the path.
        /// </para>
        /// <para>
        /// For more information see <see href="https://www.ibm.com/docs/en/spectrum-protect/8.1.9?topic=parameters-file-specification-syntax">file specification syntax</see>, <seealso href="https://en.wikipedia.org/wiki/Comparison_of_file_systems#Limits">file systems limitations</seealso>
        /// </para>
        /// </remarks>
        public const int MAX_FILENAME_LENGTH = 255 - (32 + 4 + 2 + 5); //max path - (Guid + Guid "D" format chars + Storage.CreateFileSafely chars + account for ' (99)' suffix)

        /// <summary>
        /// The file extension for exports (including the leading '.').
        /// </summary>
        protected abstract string FileExtension { get; }

        protected Storage UserFileStorage;
        private readonly Storage exportStorage;
        protected virtual string GetFilename(TModel item) => item.GetDisplayString();

        public Action<Notification>? PostNotification { get; set; }

        /// <summary>
        /// Construct exporter.
        /// Create a new exporter for each export, otherwise it will cause confusing notifications.
        /// </summary>
        /// <param name="storage">Storage for storing exported files. Basically it is used to provide export stream</param>
        protected LegacyExporter(Storage storage)
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
        public Task ExportAsync(TModel model, RealmAccess realm, CancellationToken cancellationToken = default) =>
            ExportAsync(model.ToLive(realm), cancellationToken);

        /// <summary>
        /// Export the model to default folder.
        /// </summary>
        /// <param name="model">The model should export.</param>
        /// <param name="cancellationToken">
        /// The Cancellation token that can cancel the exporting.
        /// If specified CancellationToken, then use it. Otherwise use PostNotification's CancellationToken.
        /// </param>
        /// <returns></returns>
        public async Task ExportAsync(Live<TModel> model, CancellationToken cancellationToken = default)
        {
            string itemFilename = model.PerformRead(s => GetFilename(s).GetValidFilename());

            if (itemFilename.Length > MAX_FILENAME_LENGTH - FileExtension.Length)
                itemFilename = itemFilename.Remove(MAX_FILENAME_LENGTH - FileExtension.Length);

            IEnumerable<string> existingExports = exportStorage
                                                  .GetFiles(string.Empty, $"{itemFilename}*{FileExtension}")
                                                  .Concat(exportStorage.GetDirectories(string.Empty));

            string filename = NamingUtils.GetNextBestFilename(existingExports, $"{itemFilename}{FileExtension}");

            ProgressNotification notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = $"Exporting {itemFilename}...",
            };

            PostNotification?.Invoke(notification);

            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, notification.CancellationToken);

            try
            {
                using (var stream = exportStorage.CreateFileSafely(filename))
                {
                    await ExportToStreamAsync(model, stream, notification, linkedSource.Token).ConfigureAwait(false);
                }
            }
            catch
            {
                notification.State = ProgressNotificationState.Cancelled;

                // cleanup if export is failed or canceled.
                exportStorage.Delete(filename);
                throw;
            }

            notification.CompletionText = $"Exported {itemFilename}! Click to view.";
            notification.CompletionClickAction = () => exportStorage.PresentFileExternally(filename);
            notification.State = ProgressNotificationState.Completed;
        }

        /// <summary>
        /// Export model to stream.
        /// </summary>
        /// <param name="model">The model which have <see cref="IHasGuidPrimaryKey"/>.</param>
        /// <param name="stream">The stream to export.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        /// <param name="cancellationToken">The Cancellation token that can cancel the exporting.</param>
        /// <returns>Whether the export was successful</returns>
        public Task ExportToStreamAsync(Live<TModel> model, Stream stream, ProgressNotification? notification = null, CancellationToken cancellationToken = default) =>
            Task.Run(() => { model.PerformRead(s => ExportToStream(s, stream, notification, cancellationToken)); }, cancellationToken);

        /// <summary>
        /// Exports model to Stream.
        /// </summary>
        /// <param name="model">The model to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        /// <param name="cancellationToken">The Cancellation token that can cancel the exporting.</param>
        public abstract void ExportToStream(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default);
    }
}
