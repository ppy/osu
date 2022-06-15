// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.IO.Stores;
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
            foreach (string beatmapDirectory in storage.GetDirectories(string.Empty))
            {
                var beatmapStorage = storage.GetStorageForDirectory(beatmapDirectory);

                if (!beatmapStorage.GetFiles(string.Empty).ExcludeSystemFileNames().Any())
                {
                    // if a directory doesn't contain files, attempt looking for beatmaps inside of that directory.
                    // this is a special behaviour in stable for beatmaps only, see https://github.com/ppy/osu/issues/18615.
                    foreach (string beatmapInDirectory in GetStableImportPaths(beatmapStorage))
                        yield return beatmapStorage.GetFullPath(beatmapInDirectory);
                }
                else
                    yield return storage.GetFullPath(beatmapDirectory);
            }
        }

        public LegacyBeatmapImporter(IModelImporter<BeatmapSetInfo> importer)
            : base(importer)
        {
        }
    }
}
