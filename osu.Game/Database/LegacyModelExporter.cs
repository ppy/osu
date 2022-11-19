// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
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

        protected readonly Storage ExportStorage;

        protected readonly RealmAccess RealmAccess;

        protected readonly ProgressNotification Notification;

        protected string Filename = null!;

        protected Stream? OutputStream;

        protected bool ShouldDisposeStream;

        protected LegacyModelExporter(Storage storage, RealmAccess realm, ProgressNotification notification, Stream? stream = null)
        {
            ExportStorage = storage.GetStorageForDirectory(@"exports");
            UserFileStorage = storage.GetStorageForDirectory(@"files");
            Notification = notification;
            RealmAccess = realm;
            OutputStream = stream;
            ShouldDisposeStream = false;
        }

        /// <summary>
        /// Export model to <see cref="OutputStream"/>
        /// if <see cref="OutputStream"/> is null, model will export to default folder.
        /// </summary>
        /// <param name="uuid">The model which have Guid.</param>
        /// <returns></returns>
        public virtual async Task ExportASync(IHasGuidPrimaryKey uuid)
        {
            bool canCancel = true;
            Notification.CancelRequested += () => canCancel;

            Guid id = uuid.ID;
            await Task.Run(() =>
            {
                RealmAccess.Run(r =>
                {
                    if (r.Find<TModel>(id) is IHasNamedFiles model)
                    {
                        Filename = $"{model.GetDisplayString().GetValidFilename()}{FileExtension}";
                    }
                    else
                    {
                        return;
                    }

                    if (OutputStream == null)
                    {
                        OutputStream = ExportStorage.CreateFileSafely(Filename);
                        ShouldDisposeStream = true;
                    }

                    using (var archive = ZipArchive.Create())
                    {
                        float i = 0;

                        foreach (var file in model.Files)
                        {
                            if (Notification.CancellationToken.IsCancellationRequested) return;

                            archive.AddEntry(file.Filename, UserFileStorage.GetStream(file.File.GetStoragePath()));
                            i++;
                            Notification.Progress = i / model.Files.Count();
                            Notification.Text = $"Exporting... ({i}/{model.Files.Count()})";
                        }

                        Notification.Text = "Saving Zip Archive...";
                        canCancel = false;
                        archive.SaveTo(OutputStream);
                    }
                });
            }).ContinueWith(OnComplete);
        }

        protected void OnComplete(Task t)
        {
            if (ShouldDisposeStream)
            {
                OutputStream?.Dispose();
            }

            if (t.IsFaulted)
            {
                Notification.State = ProgressNotificationState.Cancelled;
                return;
            }

            if (Notification.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            Notification.CompletionText = "Export Complete, Click to open the folder";
            Notification.CompletionClickAction += () => ExportStorage.PresentFileExternally(Filename);
            Notification.State = ProgressNotificationState.Completed;
        }
    }
}
