// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Database
{
    [ExcludeFromDynamicCompile]
    public class LegacyExportManager : Component
    {
        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private Storage exportStorage { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        public async Task ExportAsync(IHasGuidPrimaryKey item)
        {
            var notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = "Exporting...",
                CompletionText = "Export completed"
            };
            notifications?.Post(notification);

            switch (item)
            {
                case SkinInfo:
                    await new LegacySkinExporter(exportStorage, realmAccess, notification).ExportASync(item);
                    break;

                case ScoreInfo:
                    await new LegacyScoreExporter(exportStorage, realmAccess, notification).ExportASync(item);
                    break;

                case BeatmapSetInfo:
                    await new LegacyBeatmapExporter(exportStorage, realmAccess, notification).ExportASync(item);
                    break;
            }
        }
    }
}
