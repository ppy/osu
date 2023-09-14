// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Utils;
using SharpCompress.Common;

namespace osu.Game.Database
{
    /// <summary>
    /// An encapsulated import task to be imported to an <see cref="RealmArchiveModelImporter{TModel}"/>.
    /// </summary>
    public class ImportTask
    {
        /// <summary>
        /// The path to the file (or filename in the case a stream is provided).
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// An optional stream which provides the file content.
        /// </summary>
        public Stream? Stream { get; }

        /// <summary>
        /// Construct a new import task from a path (on a local filesystem).
        /// </summary>
        public ImportTask(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Construct a new import task from a stream. The provided stream will be disposed after reading.
        /// </summary>
        public ImportTask(Stream stream, string filename)
        {
            Path = filename;
            Stream = stream;
        }

        /// <summary>
        /// Retrieve an archive reader from this task.
        /// </summary>
        public ArchiveReader GetReader()
        {
            if (Stream == null)
            {
                if (ZipUtils.IsZipArchive(Path))
                    return new ZipArchiveReader(File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.Read), System.IO.Path.GetFileName(Path));
                if (Directory.Exists(Path))
                    return new DirectoryArchiveReader(Path);
                if (File.Exists(Path))
                    return new SingleFileArchiveReader(Path);

                throw new InvalidFormatException($"{Path} is not a valid archive");
            }

            if (Stream is not MemoryStream memoryStream)
            {
                // Path used primarily in tests (converting `ManifestResourceStream`s to `MemoryStream`s).
                memoryStream = new MemoryStream(Stream.ReadAllBytesToArray());
                Stream.Dispose();
            }

            if (ZipUtils.IsZipArchive(memoryStream))
                return new ZipArchiveReader(memoryStream, Path);

            return new MemoryStreamArchiveReader(memoryStream, Path);
        }

        /// <summary>
        /// Deletes the file that is encapsulated by this <see cref="ImportTask"/>.
        /// </summary>
        public virtual void DeleteFile()
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }

        public override string ToString() => System.IO.Path.GetFileName(Path);
    }
}
