// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using SharpCompress.Archives.Zip;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles exporting legacy user data of a single type from osu-stable.
    /// </summary>
    public abstract class LegacyExporter<TModel>
        where TModel : class, IHasNamedFiles
    {
        /// <summary>
        /// The file extension for exports (including the leading '.').
        /// </summary>
        protected abstract string FileExtension { get; }

        protected readonly Storage UserFileStorage;

        private readonly Storage exportStorage;

        private readonly INotificationOverlay? notificationOverlay;

        protected ProgressNotification Notification = null!;

        private string filename = null!;

        protected LegacyExporter(Storage storage, INotificationOverlay? notificationOverlay)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
            this.notificationOverlay = notificationOverlay;
        }

        /// <summary>
        /// Exports an item to a legacy (.zip based) package.
        /// </summary>
        /// <param name="item">The item to export.</param>
        public void Export(TModel item)
        {
            filename = $"{item.GetDisplayString().GetValidFilename()}{FileExtension}";

            Stream stream = exportStorage.CreateFileSafely(filename);

            Notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            Notification.CompletionClickAction += () => exportStorage.PresentFileExternally(filename);
            Notification.CancelRequested += () =>
            {
                stream.Dispose();
                return true;
            };

            ExportModelTo(item, stream);
            notificationOverlay?.Post(Notification);
        }

        /// <summary>
        /// Exports an item to the given output stream.
        /// </summary>
        /// <param name="model">The item to export.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        public virtual void ExportModelTo(TModel model, Stream outputStream)
        {
            using (var archive = ZipArchive.Create())
            {
                foreach (var file in model.Files)
                    archive.AddEntry(file.Filename, UserFileStorage.GetStream(file.File.GetStoragePath()));

                Task.Factory.StartNew(() =>
                {
                    archive.SaveTo(outputStream);
                }, Notification.CancellationToken).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        outputStream.Dispose();
                        Notification.State = ProgressNotificationState.Completed;
                    }
                    else
                    {
                        if (Notification.State == ProgressNotificationState.Cancelled) return;

                        Notification.State = ProgressNotificationState.Cancelled;
                        Notification.Text = "Export Failed";
                    }
                });
            }
        }
    }
}
