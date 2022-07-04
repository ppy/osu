// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Metadata
{
    /// <summary>
    /// Describes a set of beatmaps which have been updated in some way.
    /// </summary>
    [MessagePackObject]
    [Serializable]
    public class BeatmapUpdates
    {
        [Key(0)]
        public int[] BeatmapSetIDs { get; set; }

        [Key(1)]
        public uint LastProcessedQueueID { get; set; }

        public BeatmapUpdates(int[] beatmapSetIDs, uint lastProcessedQueueID)
        {
            BeatmapSetIDs = beatmapSetIDs;
            LastProcessedQueueID = lastProcessedQueueID;
        }
    }
}
