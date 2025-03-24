// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Edit.Submission
{
    public class SubmissionBeatmapExporter : LegacyBeatmapExporter
    {
        private readonly uint? beatmapSetId;
        private readonly HashSet<int>? allocatedBeatmapIds;

        public SubmissionBeatmapExporter(Storage storage, PutBeatmapSetResponse putBeatmapSetResponse)
            : base(storage)
        {
            beatmapSetId = putBeatmapSetResponse.BeatmapSetId;
            allocatedBeatmapIds = putBeatmapSetResponse.BeatmapIds.Select(id => (int)id).ToHashSet();
        }

        protected override void MutateBeatmap(BeatmapSetInfo beatmapSet, IBeatmap playableBeatmap)
        {
            base.MutateBeatmap(beatmapSet, playableBeatmap);

            if (beatmapSetId != null && allocatedBeatmapIds != null)
            {
                playableBeatmap.BeatmapInfo.BeatmapSet = beatmapSet;
                playableBeatmap.BeatmapInfo.BeatmapSet!.OnlineID = (int)beatmapSetId;

                if (allocatedBeatmapIds.Contains(playableBeatmap.BeatmapInfo.OnlineID))
                {
                    allocatedBeatmapIds.Remove(playableBeatmap.BeatmapInfo.OnlineID);
                    return;
                }

                if (playableBeatmap.BeatmapInfo.OnlineID > 0)
                    throw new InvalidOperationException(@"Encountered beatmap with ID that has not been assigned to it by the server!");

                if (allocatedBeatmapIds.Count == 0)
                    throw new InvalidOperationException(@"Ran out of new beatmap IDs to assign to unsubmitted beatmaps!");

                int newId = allocatedBeatmapIds.First();
                allocatedBeatmapIds.Remove(newId);
                playableBeatmap.BeatmapInfo.OnlineID = newId;
            }
        }
    }
}
