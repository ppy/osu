// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;
using Realms;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

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

        protected RealmAccess RealmAccess;

        private string filename = string.Empty;
        public Action<Notification>? PostNotification { get; set; }

        protected LegacyModelExporter(Storage storage, RealmAccess realm)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
            RealmAccess = realm;
        }

        /// <summary>
        /// Export the model to default folder.
        /// </summary>
        /// <param name="model">The model should export.</param>
        /// <returns></returns>
        public async Task ExportAsync(TModel model)
        {
            string itemFilename = model.GetDisplayString().GetValidFilename();
            IEnumerable<string> existingExports = exportStorage.GetFiles("", $"{itemFilename}*{FileExtension}");
            filename = NamingUtils.GetNextBestFilename(existingExports, $"{itemFilename}{FileExtension}");
            bool success;

            using (var stream = exportStorage.CreateFileSafely(filename))
            {
                success = await ExportToStreamAsync(model, stream);
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
        /// <returns>Whether the export was successful</returns>
        public async Task<bool> ExportToStreamAsync(TModel model, Stream stream)
        {
            ProgressNotification notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            notification.CompletionClickAction += () => exportStorage.PresentFileExternally(filename);
            PostNotification?.Invoke(notification);

            Guid id = model.ID;
            return await Task.Run(() =>
            {
                RealmAccess.Run(r =>
                {
                    TModel refetchModel = r.Find<TModel>(id);
                    ExportToStream(refetchModel, stream, notification);
                });
            }, notification.CancellationToken).ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    return false;
                }

                if (t.IsFaulted)
                {
                    notification.State = ProgressNotificationState.Cancelled;
                    Logger.Error(t.Exception, "An error occurred while exporting");
                    return false;
                }

                notification.CompletionText = "Export Complete, Click to open the folder";
                notification.State = ProgressNotificationState.Completed;
                return true;
            });
        }

        /// <summary>
        /// Exports an item to Stream.
        /// Override if custom export method is required.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        protected abstract void ExportToStream(TModel model, Stream outputStream, ProgressNotification notification);
    }

    public abstract class LegacyArchiveExporter<TModel> : LegacyModelExporter<TModel>
        where TModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
    {
        protected LegacyArchiveExporter(Storage storage, RealmAccess realm)
            : base(storage, realm)
        {
        }

        protected override void ExportToStream(TModel model, Stream outputStream, ProgressNotification notification) => exportZipArchive(model, outputStream, notification);

        /// <summary>
        /// Exports an item to Stream as a legacy (.zip based) package.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        private void exportZipArchive(TModel model, Stream outputStream, ProgressNotification notification)
        {
            using var writer = new ZipWriter(outputStream, new ZipWriterOptions(CompressionType.Deflate));

            float i = 0;
            bool fileMissing = false;

            foreach (var file in model.Files)
            {
                notification.CancellationToken.ThrowIfCancellationRequested();

                using (var stream = UserFileStorage.GetStream(file.File.GetStoragePath()))
                {
                    // Sometimes we cannot find the file(probably deleted by the user), so we handle this and post a error.
                    if (stream == null)
                    {
                        // Only pop up once to prevent spam.
                        if (!fileMissing)
                        {
                            PostNotification?.Invoke(new SimpleErrorNotification
                            {
                                Text = "Some of your files are missing, they will not be included in the archive"
                            });
                            fileMissing = true;
                        }
                    }
                    else
                    {
                        writer.Write(file.Filename, stream);
                    }
                }

                i++;
                notification.Progress = i / model.Files.Count();
                notification.Text = $"Exporting... ({i}/{model.Files.Count()})";
            }
        }
    }
}
