// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Database
{
    public partial class BeatmapLookupCache : OnlineLookupCache<int, APIBeatmap, GetBeatmapsRequest>
    {
        /// <summary>
        /// Perform an API lookup on the specified beatmap, populating a <see cref="APIBeatmap"/> model.
        /// </summary>
        /// <param name="beatmapId">The beatmap to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated beatmap, or null if the beatmap does not exist or the request could not be satisfied.</returns>
        public Task<APIBeatmap?> GetBeatmapAsync(int beatmapId, CancellationToken token = default) => LookupAsync(beatmapId, token);

        /// <summary>
        /// Perform an API lookup on the specified beatmaps, populating a <see cref="APIBeatmap"/> model.
        /// </summary>
        /// <param name="beatmapIds">The beatmaps to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated beatmaps. May include null results for failed retrievals.</returns>
        public Task<APIBeatmap?[]> GetBeatmapsAsync(int[] beatmapIds, CancellationToken token = default) => LookupAsync(beatmapIds, token);

        protected override GetBeatmapsRequest CreateRequest(IEnumerable<int> ids) => new GetBeatmapsRequest(ids.ToArray());

        protected override IEnumerable<APIBeatmap>? RetrieveResults(GetBeatmapsRequest request) => request.Response?.Beatmaps;
    }
}
