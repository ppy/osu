// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;
using Realms;
using SharpCompress.Archives.Zip;

namespace osu.Game.Database
{
    /// <summary>
    /// A class which handles exporting legacy user data of a single type from osu-stable.
    /// </summary>
    public abstract class LegacyModelExporter<TModel>
        where TModel : RealmObject
    {
        /// <summary>
        /// The file extension for exports (including the leading '.').
        /// </summary>
        protected abstract string FileExtension { get; }

        protected readonly Storage UserFileStorage;

        private readonly Storage exportStorage;

        private readonly RealmAccess realmAccess;

        private readonly ProgressNotification notification;

        protected ProgressNotification Notification = null!;

        private string filename = null!;

        protected LegacyModelExporter(Storage storage, RealmAccess realm, ProgressNotification notification)
        {
            exportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
            this.notification = notification;
            realmAccess = realm;
        }

        public async Task ExportASync(IHasGuidPrimaryKey uuid, bool needZipArchive = true)
        {
            Guid id = uuid.ID;
            await Task.Run(() =>
            {
                realmAccess.Run(r =>
                {
                    if (r.Find<TModel>(id) is IHasNamedFiles model)
                    {
                        filename = $"{model.GetDisplayString().GetValidFilename()}{FileExtension}";
                    }
                    else
                    {
                        return;
                    }

                    using (var outputStream = exportStorage.CreateFileSafely(filename))
                    {
                        if (needZipArchive)
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
                                archive.SaveTo(outputStream);
                            }
                        }
                        else
                        {
                            var file = model.Files.SingleOrDefault();
                            if (file == null)
                                return;

                            using (var inputStream = UserFileStorage.GetStream(file.File.GetStoragePath()))
                                inputStream.CopyTo(outputStream);
                        }
                    }
                });
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    notification.State = ProgressNotificationState.Cancelled;
                    return;
                }

                if (notification.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                notification.CompletionText = "Export Complete, Click to open the folder";
                notification.CompletionClickAction += () => exportStorage.PresentFileExternally(filename);
                notification.State = ProgressNotificationState.Completed;
            });
        }
    }
}
