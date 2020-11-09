// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        private readonly HashSet<int> nextTaskIDs = new HashSet<int>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly object taskAssignmentLock = new object();

        private Task<List<User>> pendingRequest;

        /// <summary>
        /// Whether <see cref="pendingRequest"/> has already grabbed its IDs.
        /// </summary>
        private bool pendingRequestConsumedIDs;

        public Task<User> GetUserAsync(int userId, CancellationToken token = default) => GetAsync(userId, token);

        protected override async Task<User> ComputeValueAsync(int lookup, CancellationToken token = default)
        {
            var users = await getQueryTaskForUser(lookup);
            return users.FirstOrDefault(u => u.Id == lookup);
        }

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
                nextTaskIDs.Add(userId);

                // if there's a pending request which hasn't been started yet (and is not yet full), we can wait on it.
                if (pendingRequest != null && !pendingRequestConsumedIDs && nextTaskIDs.Count < 50)
                    return pendingRequest;

                return queueNextTask(nextLookup);
            }

            List<User> nextLookup()
            {
                int[] lookupItems;

                lock (taskAssignmentLock)
                {
                    pendingRequestConsumedIDs = true;
                    lookupItems = nextTaskIDs.ToArray();
                    nextTaskIDs.Clear();

                    if (lookupItems.Length == 0)
                    {
                        queueNextTask(null);
                        return new List<User>();
                    }
                }

                var request = new GetUsersRequest(lookupItems);

                // rather than queueing, we maintain our own single-threaded request stream.
                api.Perform(request);

                return request.Result?.Users;
            }
        }

        /// <summary>
        /// Queues new work at the end of the current work tasks.
        /// Ensures the provided work is eventually run.
        /// </summary>
        /// <param name="work">The work to run. Can be null to signify the end of available work.</param>
        /// <returns>The task tracking this work.</returns>
        private Task<List<User>> queueNextTask(Func<List<User>> work)
        {
            lock (taskAssignmentLock)
            {
                if (work == null)
                {
                    pendingRequest = null;
                    pendingRequestConsumedIDs = false;
                }
                else if (pendingRequest == null)
                {
                    // special case for the first request ever.
                    pendingRequest = Task.Run(work);
                    pendingRequestConsumedIDs = false;
                }
                else
                {
                    // append the new request on to the last to be executed.
                    pendingRequest = pendingRequest.ContinueWith(_ => work());
                    pendingRequestConsumedIDs = false;
                }

                return pendingRequest;
            }
        }
    }
}
