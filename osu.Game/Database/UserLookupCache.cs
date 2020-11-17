// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Database
{
    public class UserLookupCache : MemoryCachingComponent<int, User>
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        public Task<User> GetUserAsync(int userId, CancellationToken token = default) => GetAsync(userId, token);

        protected override async Task<User> ComputeValueAsync(int lookup, CancellationToken token = default)
            => await queryUser(lookup);

        private readonly Queue<(int id, TaskCompletionSource<User>)> pendingUserTasks = new Queue<(int, TaskCompletionSource<User>)>();
        private Task pendingRequestTask;
        private readonly object taskAssignmentLock = new object();

        private Task<User> queryUser(int userId)
        {
            lock (taskAssignmentLock)
            {
                var tcs = new TaskCompletionSource<User>();

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
            var userTasks = new Dictionary<int, List<TaskCompletionSource<User>>>();

            // Grab at most 50 unique user IDs from the queue.
            lock (taskAssignmentLock)
            {
                while (pendingUserTasks.Count > 0 && userTasks.Count < 50)
                {
                    (int id, TaskCompletionSource<User> task) next = pendingUserTasks.Dequeue();

                    // Perform a secondary check for existence, in case the user was queried in a previous batch.
                    if (CheckExists(next.id, out var existing))
                        next.task.SetResult(existing);
                    else
                    {
                        if (userTasks.TryGetValue(next.id, out var tasks))
                            tasks.Add(next.task);
                        else
                            userTasks[next.id] = new List<TaskCompletionSource<User>> { next.task };
                    }
                }
            }

            // Query the users.
            var request = new GetUsersRequest(userTasks.Keys.ToArray());

            // rather than queueing, we maintain our own single-threaded request stream.
            api.Perform(request);

            // Create a new request task if there's still more users to query.
            lock (taskAssignmentLock)
            {
                pendingRequestTask = null;
                if (pendingUserTasks.Count > 0)
                    createNewTask();
            }

            foreach (var user in request.Result.Users)
            {
                if (userTasks.TryGetValue(user.Id, out var tasks))
                {
                    foreach (var task in tasks)
                        task.SetResult(user);

                    userTasks.Remove(user.Id);
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
