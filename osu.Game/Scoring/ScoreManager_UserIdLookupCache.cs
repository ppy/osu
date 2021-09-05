// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Database;

namespace osu.Game.Scoring
{
    public partial class ScoreManager
    {
        private class UserIdLookupCache : MemoryCachingComponent<string, int>
        {
            private readonly IAPIProvider api;

            public UserIdLookupCache(IAPIProvider api)
            {
                this.api = api;
            }

            /// <summary>
            /// Perform an API lookup on the specified username, returning the associated ID.
            /// </summary>
            /// <param name="username">The username to lookup.</param>
            /// <param name="token">An optional cancellation token.</param>
            /// <returns>The user ID, or 1 if the user does not exist or the request could not be satisfied.</returns>
            public Task<int> GetUserIdAsync(string username, CancellationToken token = default) => GetAsync(username, token);

            protected override async Task<int> ComputeValueAsync(string lookup, CancellationToken token = default)
                => await queryUserId(lookup).ConfigureAwait(false);

            private readonly Queue<(string username, TaskCompletionSource<int>)> pendingUserTasks = new Queue<(string, TaskCompletionSource<int>)>();
            private Task pendingRequestTask;
            private readonly object taskAssignmentLock = new object();

            private Task<int> queryUserId(string username)
            {
                lock (taskAssignmentLock)
                {
                    var tcs = new TaskCompletionSource<int>();

                    // Add to the queue.
                    pendingUserTasks.Enqueue((username, tcs));

                    // Create a request task if there's not already one.
                    if (pendingRequestTask == null)
                        createNewTask();

                    return tcs.Task;
                }
            }

            private void performLookup()
            {
                (string username, TaskCompletionSource<int> task) next;

                lock (taskAssignmentLock)
                {
                    next = pendingUserTasks.Dequeue();
                }

                var request = new GetUserRequest(next.username);

                // rather than queueing, we maintain our own single-threaded request stream.
                // todo: we probably want retry logic here.
                api.Perform(request);

                // Create a new request task if there's still more users to query.
                lock (taskAssignmentLock)
                {
                    pendingRequestTask = null;
                    if (pendingUserTasks.Count > 0)
                        createNewTask();
                }

                next.task.SetResult(request.Result?.Id ?? 1);
            }

            private void createNewTask() => pendingRequestTask = Task.Run(performLookup);
        }
    }
}
