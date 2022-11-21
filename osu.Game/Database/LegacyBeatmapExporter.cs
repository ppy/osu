// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Overlays;

namespace osu.Game.Database
{
    public class LegacyBeatmapExporter : LegacyModelExporter<BeatmapSetInfo>
    {
        public LegacyBeatmapExporter(Storage storage, RealmAccess realm, INotificationOverlay? notifications = null)
            : base(storage, realm, notifications)
        {
        }

        protected override string FileExtension => ".osz";
    }
}
