// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    public class LegacyBeatmapExporter : LegacyModelExporter<BeatmapSetInfo>
    {
        protected override string FileExtension => ".osz";

        public LegacyBeatmapExporter(Storage storage, RealmAccess realm, ProgressNotification notification, Stream? stream = null)
            : base(storage, realm, notification, stream)
        {
        }
    }
}
