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
    /// <summary>
    /// An exporter which handles the common scenario of exporting a model to a zip-based archive, usually with a custom file extension.
    /// </summary>
    public abstract class LegacyArchiveExporter<TModel> : LegacyExporter<TModel>
        where TModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
    {
        protected LegacyArchiveExporter(Storage storage)
            : base(storage)
        {
        }

        protected override void ExportToStream(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
            => exportZipArchive(model, outputStream, notification, cancellationToken);

        /// <summary>
        /// Exports an item to Stream as a legacy (.zip based) package.
        /// </summary>
        /// <param name="model">The model to be exported.</param>
        /// <param name="outputStream">The output stream to export to.</param>
        /// <param name="notification">An optional target notification to update with ongoing export progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private void exportZipArchive(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
        {
            try
            {
                using var writer = new ZipWriter(outputStream, new ZipWriterOptions(CompressionType.Deflate));

                int i = 0;
                int fileCount = model.Files.Count();
                bool fileMissing = false;

                foreach (var file in model.Files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var stream = UserFileStorage.GetStream(file.File.GetStoragePath()))
                    {
                        if (stream == null)
                        {
                            Logger.Log($"File {file.Filename} is missing in local storage and will not be included in the export", LoggingTarget.Database);
                            fileMissing = true;
                            continue;
                        }

                        writer.Write(file.Filename, stream);
                    }

                    if (notification != null)
                    {
                        notification.Progress = (float)(i + 1) / fileCount;
                    }

                    i++;
                }

                // Only pop up once to prevent spam.
                if (fileMissing)
                {
                    Logger.Log("Some of model files are missing, they will not be included in the archive", LoggingTarget.Database, LogLevel.Error);
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
