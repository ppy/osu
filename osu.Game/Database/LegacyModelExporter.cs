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
using osu.Game.Overlays;
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

        private readonly ProgressNotification notification;

        private bool canCancel = true;

        private readonly INotificationOverlay? notifications;

        protected LegacyModelExporter(Storage storage, RealmAccess realm, INotificationOverlay? notifications = null)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
            RealmAccess = realm;

            this.notifications = notifications;
            notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            notification.CancelRequested += () => canCancel;
        }

        /// <summary>
        /// Export the model to default folder.
        /// </summary>
        /// <param name="model">The model should export.</param>
        /// <returns></returns>
        public async Task ExportAsync(TModel model)
        {
            notifications?.Post(notification);

            string itemFilename = model.GetDisplayString().GetValidFilename();
            IEnumerable<string> existingExports = exportStorage.GetFiles("", $"{itemFilename}*{FileExtension}");
            string filename = NamingUtils.GetNextBestFilename(existingExports, $"{itemFilename}{FileExtension}");

            notification.CompletionClickAction += () => exportStorage.PresentFileExternally(filename);

            using (var stream = exportStorage.CreateFileSafely(filename))
            {
                await ExportToStreamAsync(model, stream);
            }
        }

        /// <summary>
        /// Export te model corresponding to model to given stream.
        /// </summary>
        /// <param name="model">The medel which have <see cref="IHasGuidPrimaryKey"/>.</param>
        /// <param name="stream">The stream to export.</param>
        /// <returns></returns>
        public async Task ExportToStreamAsync(TModel model, Stream stream)
        {
            Guid id = model.ID;
            await Task.Run(() =>
            {
                RealmAccess.Run(r =>
                {
                    TModel refetchModel = r.Find<TModel>(id);
                    ExportToStream(refetchModel, stream);
                });
            }).ContinueWith(onComplete);
        }

        /// <summary>
        /// Exports an item to Stream.
        /// Override if custom export method is required.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        protected virtual void ExportToStream(TModel model, Stream outputStream) => exportZipArchive(model, outputStream);

        /// <summary>
        /// Exports an item to Stream as a legacy (.zip based) package.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        private void exportZipArchive(TModel model, Stream outputStream)
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
                canCancel = false;
                archive.SaveTo(outputStream);
            }
        }

        private void onComplete(Task t)
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
        }
    }
}
