// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
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
    /// <summary>
    /// A class which centrally manage legacy file exports.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class LegacyExportManager : Component
    {
        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private Storage exportStorage { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        /// <summary>
        /// Identify the model type and and automatically assigned to the corresponding exporter.
        /// </summary>
        /// <param name="item">The model should export.</param>
        /// <param name="stream">The stream if requires a specific output-stream</param>
        /// <returns></returns>
        public async Task ExportAsync(IHasGuidPrimaryKey item, Stream? stream = null)
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
                    await new LegacySkinExporter(exportStorage, realmAccess, notification, stream).ExportAsync(item);
                    break;

                case ScoreInfo:
                    await new LegacyScoreExporter(exportStorage, realmAccess, notification, stream).ExportASync(item);
                    break;

                case BeatmapSetInfo:
                    await new LegacyBeatmapExporter(exportStorage, realmAccess, notification, stream).ExportAsync(item);
                    break;
            }
        }
    }
}
