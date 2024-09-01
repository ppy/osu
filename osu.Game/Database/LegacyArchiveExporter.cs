// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Overlays.Notifications;
using Realms;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using Logger = osu.Framework.Logging.Logger;

namespace osu.Game.Database
{
    /// <summary>
    /// Handles the common scenario of exporting a model to a zip-based archive, usually with a custom file extension.
    /// </summary>
    public abstract class LegacyArchiveExporter<TModel> : LegacyExporter<TModel>
        where TModel : RealmObject, IHasNamedFiles, IHasGuidPrimaryKey
    {
        /// <summary>
        /// Whether to always use Shift-JIS encoding for archive filenames (like osu!stable did).
        /// </summary>
        protected virtual bool UseFixedEncoding => true;

        protected LegacyArchiveExporter(Storage storage)
            : base(storage)
        {
        }

        public override void ExportToStream(TModel model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
        {
            var zipWriterOptions = new ZipWriterOptions(CompressionType.Deflate)
            {
                ArchiveEncoding = UseFixedEncoding ? ZipArchiveReader.DEFAULT_ENCODING : new ArchiveEncoding(Encoding.UTF8, Encoding.UTF8)
            };

            using (var writer = new ZipWriter(outputStream, zipWriterOptions))
            {
                int i = 0;
                int fileCount = model.Files.Count();
                bool anyFileMissing = false;

                foreach (var file in model.Files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using (var stream = GetFileContents(model, file))
                    {
                        if (stream == null)
                        {
                            Logger.Log($"File {file.Filename} is missing in local storage and will not be included in the export", LoggingTarget.Database);
                            anyFileMissing = true;
                            continue;
                        }

                        writer.Write(file.Filename, stream);
                    }

                    i++;

                    if (notification != null)
                    {
                        notification.Progress = (float)i / fileCount;
                    }
                }

                if (anyFileMissing)
                {
                    Logger.Log("Some files are missing in local storage and will not be included in the export", LoggingTarget.Database, LogLevel.Error);
                }
            }
        }

        protected virtual Stream? GetFileContents(TModel model, INamedFileUsage file) => UserFileStorage.GetStream(file.File.GetStoragePath());
    }
}
