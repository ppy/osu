// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Graphics;

namespace osu.Game.Online.Metadata
{
    public abstract class MetadataClient : Component, IMetadataClient, IMetadataServer
    {
        public abstract Task BeatmapSetsUpdated(BeatmapUpdates updates);

        public abstract Task<BeatmapUpdates> GetChangesSince(int queueId);

        public Action<int[]>? ChangedBeatmapSetsArrived;

        protected Task ProcessChanges(int[] beatmapSetIDs)
        {
            ChangedBeatmapSetsArrived?.Invoke(beatmapSetIDs.Distinct().ToArray());
            return Task.CompletedTask;
        }
    }
}
