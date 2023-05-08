// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Beatmaps;

namespace osu.Game.Database
{
    public class LegacyBeatmapExporter : LegacyArchiveExporter<BeatmapSetInfo>
    {
        public LegacyBeatmapExporter(Storage storage)
            : base(storage)
        {
        }

        protected override string FileExtension => @".osz";
    }
}
