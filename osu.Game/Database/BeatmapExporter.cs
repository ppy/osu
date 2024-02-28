// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Beatmaps;

namespace osu.Game.Database
{
    /// <summary>
    /// Exporter for beatmap archives.
    /// This is not for legacy purposes and works for lazer only.
    /// </summary>
    public class BeatmapExporter : LegacyArchiveExporter<BeatmapSetInfo>
    {
        public BeatmapExporter(Storage storage)
            : base(storage)
        {
        }

        protected override string FileExtension => @".olz";
    }
}
