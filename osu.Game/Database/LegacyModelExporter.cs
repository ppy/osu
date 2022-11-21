// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using Realms;
using SharpCompress.Archives.Zip;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles exporting legacy user data of a single type from osu-stable.
    /// </summary>
    public abstract class LegacyModelExporter<TModel> : Component
        where TModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
    {
        /// <summary>
        /// The file extension for exports (including the leading '.').
        /// </summary>
        protected abstract string FileExtension { get; }

        protected Storage UserFileStorage;
        protected Storage ExportStorage;

        protected RealmAccess RealmAccess;

        private readonly ProgressNotification notification;

        protected string Filename = null!;

        private bool canCancel = true;

        protected LegacyModelExporter(Storage storage, RealmAccess realm, INotificationOverlay? notifications = null)
        {
            ExportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
            RealmAccess = realm;

            notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            notification.CancelRequested += () => canCancel;

            notifications?.Post(notification);
        }

        /// <summary>
        /// Export the model to default folder.
        /// </summary>
        /// <param name="item">The model should export.</param>
        /// <returns></returns>
        public async Task ExportAsync(RealmObject item)
        {
            if (item is TModel model)
            {
                Filename = $"{model.GetDisplayString().GetValidFilename()}{FileExtension}";

                using (var stream = ExportStorage.CreateFileSafely(Filename))
                {
                    await ExportToStreamAsync(model, stream);
                }
            }
        }

        /// <summary>
        /// Export te model corresponding to uuid to given stream.
        /// </summary>
        /// <param name="uuid">The medel which have <see cref="IHasGuidPrimaryKey"/>.</param>
        /// <param name="stream">The stream to export.</param>
        /// <returns></returns>
        public virtual async Task ExportToStreamAsync(TModel uuid, Stream stream)
        {
            Guid id = uuid.ID;
            await Task.Run(() =>
            {
                RealmAccess.Run(r =>
                {
                    TModel model = r.Find<TModel>(id);
                    createZipArchive(model, stream);
                });
            }).ContinueWith(OnComplete);
        }

        private void createZipArchive(TModel model, Stream outputStream)
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

        protected void OnComplete(Task t)
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
            notification.CompletionClickAction += () => ExportStorage.PresentFileExternally(Filename);
            notification.State = ProgressNotificationState.Completed;
        }
    }
}
