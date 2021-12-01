// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Database
{
    // This class is based on `UserLookupCache` which is well tested.
    // If modifications are to be made here, a base abstract implementation should likely be created and shared between the two.
    public class BeatmapLookupCache : MemoryCachingComponent<int, APIBeatmap>
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        /// <summary>
        /// Perform an API lookup on the specified beatmap, populating a <see cref="APIBeatmap"/> model.
        /// </summary>
        /// <param name="beatmapId">The beatmap to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated beatmap, or null if the beatmap does not exist or the request could not be satisfied.</returns>
        [ItemCanBeNull]
        public Task<APIBeatmap> GetBeatmapAsync(int beatmapId, CancellationToken token = default) => GetAsync(beatmapId, token);

        /// <summary>
        /// Perform an API lookup on the specified beatmaps, populating a <see cref="APIBeatmap"/> model.
        /// </summary>
        /// <param name="beatmapIds">The beatmaps to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated beatmaps. May include null results for failed retrievals.</returns>
        public Task<APIBeatmap[]> GetBeatmapsAsync(int[] beatmapIds, CancellationToken token = default)
        {
            var beatmapLookupTasks = new List<Task<APIBeatmap>>();

            foreach (int u in beatmapIds)
            {
                beatmapLookupTasks.Add(GetBeatmapAsync(u, token).ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                        return null;

                    return task.Result;
                }, token));
            }

            return Task.WhenAll(beatmapLookupTasks);
        }

        protected override async Task<APIBeatmap> ComputeValueAsync(int lookup, CancellationToken token = default)
            => await queryBeatmap(lookup).ConfigureAwait(false);

        private readonly Queue<(int id, TaskCompletionSource<APIBeatmap>)> pendingBeatmapTasks = new Queue<(int, TaskCompletionSource<APIBeatmap>)>();
        private Task pendingRequestTask;
        private readonly object taskAssignmentLock = new object();

        private Task<APIBeatmap> queryBeatmap(int beatmapId)
        {
            lock (taskAssignmentLock)
            {
                var tcs = new TaskCompletionSource<APIBeatmap>();

                // Add to the queue.
                pendingBeatmapTasks.Enqueue((beatmapId, tcs));

                // Create a request task if there's not already one.
                if (pendingRequestTask == null)
                    createNewTask();

                return tcs.Task;
            }
        }

        private void performLookup()
        {
            // contains at most 50 unique beatmap IDs from beatmapTasks, which is used to perform the lookup.
            var beatmapTasks = new Dictionary<int, List<TaskCompletionSource<APIBeatmap>>>();

            // Grab at most 50 unique beatmap IDs from the queue.
            lock (taskAssignmentLock)
            {
                while (pendingBeatmapTasks.Count > 0 && beatmapTasks.Count < 50)
                {
                    (int id, TaskCompletionSource<APIBeatmap> task) next = pendingBeatmapTasks.Dequeue();

                    // Perform a secondary check for existence, in case the beatmap was queried in a previous batch.
                    if (CheckExists(next.id, out var existing))
                        next.task.SetResult(existing);
                    else
                    {
                        if (beatmapTasks.TryGetValue(next.id, out var tasks))
                            tasks.Add(next.task);
                        else
                            beatmapTasks[next.id] = new List<TaskCompletionSource<APIBeatmap>> { next.task };
                    }
                }
            }

            // Query the beatmaps.
            var request = new GetBeatmapsRequest(beatmapTasks.Keys.ToArray());

            // rather than queueing, we maintain our own single-threaded request stream.
            // todo: we probably want retry logic here.
            api.Perform(request);

            // Create a new request task if there's still more beatmaps to query.
            lock (taskAssignmentLock)
            {
                pendingRequestTask = null;
                if (pendingBeatmapTasks.Count > 0)
                    createNewTask();
            }

            List<APIBeatmap> foundBeatmaps = request.Response?.Beatmaps;

            if (foundBeatmaps != null)
            {
                foreach (var beatmap in foundBeatmaps)
                {
                    if (beatmapTasks.TryGetValue(beatmap.OnlineID, out var tasks))
                    {
                        foreach (var task in tasks)
                            task.SetResult(beatmap);

                        beatmapTasks.Remove(beatmap.OnlineID);
                    }
                }
            }

            // if any tasks remain which were not satisfied, return null.
            foreach (var tasks in beatmapTasks.Values)
            {
                foreach (var task in tasks)
                    task.SetResult(null);
            }
        }

        private void createNewTask() => pendingRequestTask = Task.Run(performLookup);
    }
}
