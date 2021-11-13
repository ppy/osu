// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.IO;
using osu.Game.IO.Archives;
using osu.Game.Utils;
using SharpCompress.Common;

namespace osu.Game.Database
{
    /// <summary>
    /// An encapsulated import task to be imported to an <see cref="ArchiveModelManager{TModel,TFileModel}"/>.
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
        /// Construct a new import task from a stream.
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
            return Stream != null
                ? getReaderFrom(Stream)
                : getReaderFrom(Path);
        }

        /// <summary>
        /// Creates an <see cref="ArchiveReader"/> from a stream.
        /// </summary>
        /// <param name="stream">A seekable stream containing the archive content.</param>
        /// <returns>A reader giving access to the archive's content.</returns>
        private ArchiveReader getReaderFrom(Stream stream)
        {
            if (!(stream is MemoryStream memoryStream))
            {
                // This isn't used in any current path. May need to reconsider for performance reasons (ie. if we don't expect the incoming stream to be copied out).
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                memoryStream = new MemoryStream(buffer);
            }

            if (ZipUtils.IsZipArchive(memoryStream))
                return new ZipArchiveReader(memoryStream, Path);

            return new LegacyByteArrayReader(memoryStream.ToArray(), Path);
        }

        /// <summary>
        /// Creates an <see cref="ArchiveReader"/> from a valid storage path.
        /// </summary>
        /// <param name="path">A file or folder path resolving the archive content.</param>
        /// <returns>A reader giving access to the archive's content.</returns>
        private ArchiveReader getReaderFrom(string path)
        {
            if (ZipUtils.IsZipArchive(path))
                return new ZipArchiveReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), System.IO.Path.GetFileName(path));
            if (Directory.Exists(path))
                return new LegacyDirectoryArchiveReader(path);
            if (File.Exists(path))
                return new LegacyFileArchiveReader(path);

            throw new InvalidFormatException($"{path} is not a valid archive");
        }

        public override string ToString() => System.IO.Path.GetFileName(Path);
    }
}
