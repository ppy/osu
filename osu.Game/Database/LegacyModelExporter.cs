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
using SharpCompress.Archives.Zip;

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

        protected bool CanCancel = true;

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

            using (var stream = exportStorage.CreateFileSafely(filename))
            {
                await ExportToStreamAsync(model, stream);
            }
        }

        /// <summary>
        /// Export model to stream.
        /// </summary>
        /// <param name="model">The medel which have <see cref="IHasGuidPrimaryKey"/>.</param>
        /// <param name="stream">The stream to export.</param>
        /// <returns></returns>
        public async Task ExportToStreamAsync(TModel model, Stream stream)
        {
            ProgressNotification notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            notification.CompletionClickAction += () => exportStorage.PresentFileExternally(filename);
            notification.CancelRequested += () => CanCancel;
            PostNotification?.Invoke(notification);
            CanCancel = true;

            Guid id = model.ID;
            await Task.Run(() =>
            {
                RealmAccess.Run(r =>
                {
                    TModel refetchModel = r.Find<TModel>(id);
                    ExportToStream(refetchModel, stream, notification);
                });
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    notification.State = ProgressNotificationState.Cancelled;
                    Logger.Error(t.Exception, "An error occurred while exporting");

                    return;
                }

                if (notification.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                notification.CompletionText = "Export Complete, Click to open the folder";
                notification.State = ProgressNotificationState.Completed;
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
            using (var archive = ZipArchive.Create())
            {
                float i = 0;

                foreach (var file in model.Files)
                {
                    if (notification.CancellationToken.IsCancellationRequested) return;

                    archive.AddEntry(file.Filename, UserFileStorage.GetStream(file.File.GetStoragePath()));
                    i++;
                    notification.Progress = i / model.Files.Count();
                    notification.Text = $"Exporting... ({i}/{model.Files.Count()})";
                }

                notification.Text = "Saving Zip Archive...";
                CanCancel = false;
                archive.SaveTo(outputStream);
            }
        }
    }
}
