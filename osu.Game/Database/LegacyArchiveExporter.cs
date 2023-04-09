// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;
using Realms;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using Logger = osu.Framework.Logging.Logger;

namespace osu.Game.Database
{
    public abstract class LegacyArchiveExporter<TModel> : LegacyModelExporter<TModel>
        where TModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
    {
        protected LegacyArchiveExporter(Storage storage, RealmAccess realm)
            : base(storage, realm)
        {
        }

        protected override void ExportToStream(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
            => exportZipArchive(model, outputStream, notification, cancellationToken);

        /// <summary>
        /// Exports an item to Stream as a legacy (.zip based) package.
        /// </summary>
        /// <param name="model">The model will be exported.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">The notification will displayed to the user</param>
        /// <param name="cancellationToken">The Cancellation token that can cancel the exporting.</param>
        private void exportZipArchive(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
        {
            try
            {
                using var writer = new ZipWriter(outputStream, new ZipWriterOptions(CompressionType.Deflate));

                float i = 0;
                bool fileMissing = false;

                foreach (var file in model.Files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var stream = UserFileStorage.GetStream(file.File.GetStoragePath()))
                    {
                        // Sometimes we cannot find the file(probably deleted by the user), so we handle this and post a error.
                        if (stream == null)
                        {
                            // Only pop up once to prevent spam.
                            if (!fileMissing)
                            {
                                Logger.Log("Some of model files are missing, they will not be included in the archive", LoggingTarget.Database, LogLevel.Error);
                                fileMissing = true;
                            }
                        }
                        else
                        {
                            writer.Write(file.Filename, stream);
                        }
                    }

                    i++;

                    if (notification != null)
                    {
                        notification.Progress = i / model.Files.Count();
                        notification.Text = $"Exporting... ({i}/{model.Files.Count()})";
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // outputStream may close before writing when request cancel.
                if (cancellationToken.IsCancellationRequested)
                    return;

                throw;
            }
        }
    }
}
