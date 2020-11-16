// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly object taskAssignmentLock = new object();

        public Task<User> GetUserAsync(int userId, CancellationToken token = default) => GetAsync(userId, token);

        protected override async Task<User> ComputeValueAsync(int lookup, CancellationToken token = default)
        {
            var users = await getQueryTaskForUser(lookup);
            return users.FirstOrDefault(u => u.Id == lookup);
        }

        private readonly Queue<LookupTask> tasks = new Queue<LookupTask>();

        /// <summary>
        /// Return the task responsible for fetching the provided user.
        /// This may be part of a larger batch lookup to reduce web requests.
        /// </summary>
        /// <param name="userId">The user to lookup.</param>
        /// <returns>The task responsible for the lookup.</returns>
        private Task<List<User>> getQueryTaskForUser(int userId)
        {
            lock (taskAssignmentLock)
            {
                // attempt to queue on the next pending task.
                var lastTask = tasks.LastOrDefault();

                if (lastTask?.AddUser(userId) == true)
                    return lastTask.Task;

                var lookup = new LookupTask(api);

                // always start the next task running when a previous task finishes.
                lookup.Task.ContinueWith(checkNextTaskInQueue);

                bool added = lookup.AddUser(userId);

                Debug.Assert(added);

                tasks.Enqueue(lookup);

                // in the case this is the first task to be queued, run immediately.
                if (tasks.Count == 1)
                    startNextTask();

                return lookup.Task;
            }
        }

        /// <summary>
        /// Dequeue a completed task and start the next pending task.
        /// </summary>
        /// <param name="completed">The imminently completed task.</param>
        private void checkNextTaskInQueue(Task<List<User>> completed)
        {
            lock (taskAssignmentLock)
            {
                var dequeued = tasks.Dequeue();
                Debug.Assert(completed == dequeued.Task);

                startNextTask();
            }
        }

        /// <summary>
        /// Starts the next task in the queue, given there is any pending task.
        /// </summary>
        private void startNextTask()
        {
            lock (taskAssignmentLock)
            {
                if (tasks.TryPeek(out var task))
                {
                    Debug.Assert(task.Task.Status == TaskStatus.Created);
                    task.Task.Start();
                }
            }
        }

        private class LookupTask
        {
            /// <summary>
            /// The task to be performed.
            /// </summary>
            public readonly Task<List<User>> Task;

            private readonly IAPIProvider api;

            /// <summary>
            /// Locked flag to ensure no user IDs are added after the task has consumed them.
            /// </summary>
            private bool wasRun;

            private readonly HashSet<int> users = new HashSet<int>();

            private readonly object lockObject = new object();

            public LookupTask(IAPIProvider api)
            {
                this.api = api;
                Task = new Task<List<User>>(perform);
            }

            /// <summary>
            /// Attempt to queue a user ID to this lookup task.
            /// </summary>
            /// <param name="id"></param>
            /// <returns>Whether the user could be queued. If false, a new task should be used instead.</returns>
            public bool AddUser(int id)
            {
                lock (lockObject)
                {
                    if (wasRun)
                        return false;

                    if (users.Count >= 50)
                        return false;

                    users.Add(id);
                    return true;
                }
            }

            private List<User> perform()
            {
                lock (lockObject)
                {
                    Debug.Assert(!wasRun);
                    wasRun = true;
                }

                Debug.Assert(users.Count <= 50);

                var request = new GetUsersRequest(users.ToArray());

                // rather than queueing, we maintain our own single-threaded request stream.
                api.Perform(request);

                return request.Result?.Users ?? new List<User>();
            }
        }
    }
}
