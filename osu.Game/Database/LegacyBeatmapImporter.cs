// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.IO;

namespace osu.Game.Database
{
    public class LegacyBeatmapImporter : LegacyModelImporter<BeatmapSetInfo>
    {
        protected override string ImportFromStablePath => ".";

        protected override Storage PrepareStableStorage(StableStorage stableStorage) => stableStorage.GetSongStorage();

        protected override IEnumerable<string> GetStableImportPaths(Storage storage)
        {
            // make sure the directory exists
            if (!storage.ExistsDirectory(string.Empty))
                return Array.Empty<string>();

            List<string> paths = new List<string>();

            try
            {
                foreach (string directory in storage.GetDirectories(string.Empty))
                {
                    var directoryStorage = storage.GetStorageForDirectory(directory);

                    try
                    {
                        if (!directoryStorage.GetFiles(string.Empty, "*.osu").Any())
                        {
                            // if a directory doesn't contain any beatmap files, look for further nested beatmap directories.
                            // this is a special behaviour in stable for beatmaps only, see https://github.com/ppy/osu/issues/18615.
                            foreach (string subDirectory in GetStableImportPaths(directoryStorage))
                                paths.Add(subDirectory);
                        }
                        else
                            paths.Add(storage.GetFullPath(directory));
                    }
                    catch (Exception e)
                    {
                        // Catch any errors when enumerating files
                        Logger.Log($"Error when enumerating files in {directoryStorage.GetFullPath(string.Empty)}: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                // Catch any errors when enumerating directories
                Logger.Log($"Error when enumerating directories in {storage.GetFullPath(string.Empty)}: {e}");
            }

            return paths;
        }

        /// <summary>
        /// Reads the <c>osu!.db</c> from the stable installation root and returns a mapping of
        /// folder name (relative to Songs, case-insensitive) to the date the beatmap set was added.
        /// Returns <see langword="null"/> when the database is unavailable or cannot be parsed.
        /// </summary>
        protected virtual Dictionary<string, DateTimeOffset>? ReadDateAddedFromStableDb(StableStorage stableStorage)
            => OsuDbReader.ReadDateAddedByFolder(stableStorage);

        protected override IEnumerable<ImportTask> CreateImportTasks(Storage songsStorage, StableStorage stableStorage)
        {
            var dateAddedByFolder = ReadDateAddedFromStableDb(stableStorage);

            foreach (string path in GetStableImportPaths(songsStorage))
            {
                var task = new ImportTask(path);

                if (dateAddedByFolder != null)
                {
                    // The folder name stored in osu!.db is relative to the Songs directory.
                    // For nested folders (e.g., "subdirectory\beatmap"), we need to compute the relative path.
                    string songsRoot = songsStorage.GetFullPath(string.Empty);
                    string? relativePath = Path.GetRelativePath(songsRoot, path);

                    if (!string.IsNullOrEmpty(relativePath) && relativePath != ".")
                    {
                        // Normalize path separators for cross-platform compatibility.
                        // osu!.db on Windows uses backslash, but we may be importing on Linux/macOS.
                        // Try both forward and backward slash variants.
                        string normalizedPath = relativePath.Replace('/', '\\');
                        string alternativePath = relativePath.Replace('\\', '/');

                        if (dateAddedByFolder.TryGetValue(normalizedPath, out var dateAdded))
                            task.DateAdded = dateAdded;
                        else if (dateAddedByFolder.TryGetValue(alternativePath, out dateAdded))
                            task.DateAdded = dateAdded;
                        else if (dateAddedByFolder.TryGetValue(relativePath, out dateAdded))
                            task.DateAdded = dateAdded;
                    }
                }

                yield return task;
            }
        }

        public LegacyBeatmapImporter(IModelImporter<BeatmapSetInfo> importer)
            : base(importer)
        {
        }
    }
}
