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
    public class UserLookupCache : MemoryCachingComponent<int, APIUser>
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        /// <summary>
        /// Perform an API lookup on the specified user, populating a <see cref="APIUser"/> model.
        /// </summary>
        /// <param name="userId">The user to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated user, or null if the user does not exist or the request could not be satisfied.</returns>
        [ItemCanBeNull]
        public Task<APIUser> GetUserAsync(int userId, CancellationToken token = default) => GetAsync(userId, token);

        /// <summary>
        /// Perform an API lookup on the specified users, populating a <see cref="APIUser"/> model.
        /// </summary>
        /// <param name="userIds">The users to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated users. May include null results for failed retrievals.</returns>
        public Task<APIUser[]> GetUsersAsync(int[] userIds, CancellationToken token = default)
        {
            var userLookupTasks = new List<Task<APIUser>>();

            foreach (int u in userIds)
            {
                userLookupTasks.Add(GetUserAsync(u, token).ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                        return null;

                    return task.Result;
                }, token));
            }

            return Task.WhenAll(userLookupTasks);
        }

        protected override async Task<APIUser> ComputeValueAsync(int lookup, CancellationToken token = default)
            => await queryUser(lookup).ConfigureAwait(false);

        private readonly Queue<(int id, TaskCompletionSource<APIUser>)> pendingUserTasks = new Queue<(int, TaskCompletionSource<APIUser>)>();
        private Task pendingRequestTask;
        private readonly object taskAssignmentLock = new object();

        private Task<APIUser> queryUser(int userId)
        {
            lock (taskAssignmentLock)
            {
                var tcs = new TaskCompletionSource<APIUser>();

                // Add to the queue.
                pendingUserTasks.Enqueue((userId, tcs));

                // Create a request task if there's not already one.
                if (pendingRequestTask == null)
                    createNewTask();

                return tcs.Task;
            }
        }

        private void performLookup()
        {
            // contains at most 50 unique user IDs from userTasks, which is used to perform the lookup.
            var userTasks = new Dictionary<int, List<TaskCompletionSource<APIUser>>>();

            // Grab at most 50 unique user IDs from the queue.
            lock (taskAssignmentLock)
            {
                while (pendingUserTasks.Count > 0 && userTasks.Count < 50)
                {
                    (int id, TaskCompletionSource<APIUser> task) next = pendingUserTasks.Dequeue();

                    // Perform a secondary check for existence, in case the user was queried in a previous batch.
                    if (CheckExists(next.id, out var existing))
                        next.task.SetResult(existing);
                    else
                    {
                        if (userTasks.TryGetValue(next.id, out var tasks))
                            tasks.Add(next.task);
                        else
                            userTasks[next.id] = new List<TaskCompletionSource<APIUser>> { next.task };
                    }
                }
            }

            // Query the users.
            var request = new GetUsersRequest(userTasks.Keys.ToArray());

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

            List<APIUser> foundUsers = request.Response?.Users;

            if (foundUsers != null)
            {
                foreach (var user in foundUsers)
                {
                    if (userTasks.TryGetValue(user.Id, out var tasks))
                    {
                        foreach (var task in tasks)
                            task.SetResult(user);

                        userTasks.Remove(user.Id);
                    }
                }
            }

            // if any tasks remain which were not satisfied, return null.
            foreach (var tasks in userTasks.Values)
            {
                foreach (var task in tasks)
                    task.SetResult(null);
            }
        }

        private void createNewTask() => pendingRequestTask = Task.Run(performLookup);
    }
}
