// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        public LegacyBeatmapImporter(IModelImporter<BeatmapSetInfo> importer)
            : base(importer)
        {
        }
    }
}
