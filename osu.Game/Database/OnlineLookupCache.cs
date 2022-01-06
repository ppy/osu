// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Online.API;

namespace osu.Game.Database
{
    public abstract class OnlineLookupCache<TLookup, TValue, TRequest> : MemoryCachingComponent<TLookup, TValue>
        where TLookup : IEquatable<TLookup>
        where TValue : class, IHasOnlineID<TLookup>
        where TRequest : APIRequest
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        /// <summary>
        /// Creates an <see cref="APIRequest"/> to retrieve the values for a given collection of <typeparamref name="TLookup"/>s.
        /// </summary>
        /// <param name="ids">The IDs to perform the lookup with.</param>
        protected abstract TRequest CreateRequest(IEnumerable<TLookup> ids);

        /// <summary>
        /// Retrieves a list of <typeparamref name="TValue"/>s from a successful <typeparamref name="TRequest"/> created by <see cref="CreateRequest"/>.
        /// </summary>
        [CanBeNull]
        protected abstract IEnumerable<TValue> RetrieveResults(TRequest request);

        /// <summary>
        /// Perform a lookup using the specified <paramref name="id"/>, populating a <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="id">The ID to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated <typeparamref name="TValue"/>, or null if the value does not exist or the request could not be satisfied.</returns>
        [ItemCanBeNull]
        protected Task<TValue> LookupAsync(TLookup id, CancellationToken token = default) => GetAsync(id, token);

        /// <summary>
        /// Perform an API lookup on the specified <paramref name="ids"/>, populating a <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="ids">The IDs to lookup.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>The populated values. May include null results for failed retrievals.</returns>
        protected Task<TValue[]> LookupAsync(TLookup[] ids, CancellationToken token = default)
        {
            var lookupTasks = new List<Task<TValue>>();

            foreach (var id in ids)
            {
                lookupTasks.Add(LookupAsync(id, token).ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                        return null;

                    return task.GetResultSafely();
                }, token));
            }

            return Task.WhenAll(lookupTasks);
        }

        // cannot be sealed due to test usages (see TestUserLookupCache).
        protected override async Task<TValue> ComputeValueAsync(TLookup lookup, CancellationToken token = default)
            => await queryValue(lookup).ConfigureAwait(false);

        private readonly Queue<(TLookup id, TaskCompletionSource<TValue>)> pendingTasks = new Queue<(TLookup, TaskCompletionSource<TValue>)>();
        private Task pendingRequestTask;
        private readonly object taskAssignmentLock = new object();

        private Task<TValue> queryValue(TLookup id)
        {
            lock (taskAssignmentLock)
            {
                var tcs = new TaskCompletionSource<TValue>();

                // Add to the queue.
                pendingTasks.Enqueue((id, tcs));

                // Create a request task if there's not already one.
                if (pendingRequestTask == null)
                    createNewTask();

                return tcs.Task;
            }
        }

        private void performLookup()
        {
            // contains at most 50 unique IDs from tasks, which is used to perform the lookup.
            var nextTaskBatch = new Dictionary<TLookup, List<TaskCompletionSource<TValue>>>();

            // Grab at most 50 unique IDs from the queue.
            lock (taskAssignmentLock)
            {
                while (pendingTasks.Count > 0 && nextTaskBatch.Count < 50)
                {
                    (TLookup id, TaskCompletionSource<TValue> task) next = pendingTasks.Dequeue();

                    // Perform a secondary check for existence, in case the value was queried in a previous batch.
                    if (CheckExists(next.id, out var existing))
                        next.task.SetResult(existing);
                    else
                    {
                        if (nextTaskBatch.TryGetValue(next.id, out var tasks))
                            tasks.Add(next.task);
                        else
                            nextTaskBatch[next.id] = new List<TaskCompletionSource<TValue>> { next.task };
                    }
                }
            }

            if (nextTaskBatch.Count == 0)
            {
                finishPendingTask();
                return;
            }

            // Query the values.
            var request = CreateRequest(nextTaskBatch.Keys.ToArray());

            // rather than queueing, we maintain our own single-threaded request stream.
            // todo: we probably want retry logic here.
            api.Perform(request);

            finishPendingTask();

            var foundValues = RetrieveResults(request);

            if (foundValues != null)
            {
                foreach (var value in foundValues)
                {
                    if (nextTaskBatch.TryGetValue(value.OnlineID, out var tasks))
                    {
                        foreach (var task in tasks)
                            task.SetResult(value);

                        nextTaskBatch.Remove(value.OnlineID);
                    }
                }
            }

            // if any tasks remain which were not satisfied, return null.
            foreach (var tasks in nextTaskBatch.Values)
            {
                foreach (var task in tasks)
                    task.SetResult(null);
            }
        }

        private void finishPendingTask()
        {
            // Create a new request task if there's still more values to query.
            lock (taskAssignmentLock)
            {
                pendingRequestTask = null;
                if (pendingTasks.Count > 0)
                    createNewTask();
            }
        }

        private void createNewTask() => pendingRequestTask = Task.Run(performLookup);
    }
}
