// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using System.IO;

namespace osu.Game.Database
{
    public class TemporaryBeatmapExporter : LegacyExporter<BeatmapSetInfo>
    {

        protected override string FileExtension => ".osz";
        private readonly Storage temporaryStorage;

        public TemporaryBeatmapExporter(Storage storage)
            : base(storage)
        {
            temporaryStorage = storage.GetStorageForDirectory(@"temps");
        }

        public override void Export(BeatmapSetInfo item)
        {
            string directoryName = $"{item.GetDisplayString().GetValidArchiveContentFilename()}";
            Storage directoryStorage = temporaryStorage.GetStorageForDirectory(directoryName);

            foreach (var file in item.Files)
            {
                using (var outputStream = directoryStorage.CreateFileSafely(file.Filename))
                    UserFileStorage.GetStream(file.File.GetStoragePath()).CopyTo(outputStream);
            }
            
            directoryStorage.PresentExternally();

        }
    }
}
