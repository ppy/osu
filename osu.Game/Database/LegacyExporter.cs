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
    /// Handles exporting models to files for sharing / consumption outside the game.
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

        protected readonly Storage UserFileStorage;
        private readonly Storage exportStorage;

        public Action<Notification>? PostNotification { get; set; }

        protected LegacyExporter(Storage storage)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
        }

        /// <summary>
        /// Returns the baseline name of the file to which the <paramref name="item"/> will be exported.
        /// </summary>
        /// <remarks>
        /// The name of the file will be run through <see cref="ModelExtensions.GetValidFilename"/> to eliminate characters
        /// which are not permitted by various filesystems.
        /// </remarks>
        /// <param name="item">The item being exported.</param>
        protected virtual string GetFilename(TModel item) => item.GetDisplayString();

        /// <summary>
        /// Exports a model to the default export location.
        /// This will create a notification tracking the progress of the export, visible to the user.
        /// </summary>
        /// <param name="model">The model to export.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
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
        /// Exports a model to a provided stream.
        /// </summary>
        /// <param name="model">The model to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">An optional notification to be updated with export progress.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public Task ExportToStreamAsync(Live<TModel> model, Stream outputStream, ProgressNotification? notification = null, CancellationToken cancellationToken = default) =>
            Task.Run(() => { model.PerformRead(s => ExportToStream(s, outputStream, notification, cancellationToken)); }, cancellationToken);

        /// <summary>
        /// Exports a model to a provided stream.
        /// </summary>
        /// <param name="model">The model to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">An optional notification to be updated with export progress.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public abstract void ExportToStream(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default);
    }
}
