// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Online.Metadata;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Ingests any changes that happen externally to the client, reprocessing as required.
    /// </summary>
    public partial class BeatmapOnlineChangeIngest : Component
    {
        private readonly BeatmapUpdater beatmapUpdater;
        private readonly RealmAccess realm;
        private readonly MetadataClient metadataClient;

        public BeatmapOnlineChangeIngest(BeatmapUpdater beatmapUpdater, RealmAccess realm, MetadataClient metadataClient)
        {
            this.beatmapUpdater = beatmapUpdater;
            this.realm = realm;
            this.metadataClient = metadataClient;

            metadataClient.ChangedBeatmapSetsArrived += changesDetected;
        }

        private void changesDetected(int[] beatmapSetIds)
        {
            // May want to batch incoming updates further if the background realm operations ever becomes a concern.
            realm.Run(r =>
            {
                foreach (int id in beatmapSetIds)
                {
                    var matchingSet = r.All<BeatmapSetInfo>().FirstOrDefault(s => s.OnlineID == id);

                    if (matchingSet != null)
                        beatmapUpdater.Queue(matchingSet.ToLive(realm), MetadataLookupScope.OnlineFirst);
                }
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            metadataClient.ChangedBeatmapSetsArrived -= changesDetected;
        }
    }
}
