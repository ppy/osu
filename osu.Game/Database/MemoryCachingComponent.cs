// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Graphics;

namespace osu.Game.Database
{
    /// <summary>
    /// A component which performs lookups (or calculations) and caches the results.
    /// Currently not persisted between game sessions.
    /// </summary>
    public abstract class MemoryCachingComponent<TLookup, TValue> : Component
    {
        private readonly ConcurrentDictionary<TLookup, TValue> cache = new ConcurrentDictionary<TLookup, TValue>();

        protected virtual bool CacheNullValues => true;

        /// <summary>
        /// Retrieve the cached value for the given lookup.
        /// </summary>
        /// <param name="lookup">The lookup to retrieve.</param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        protected async Task<TValue> GetAsync([NotNull] TLookup lookup, CancellationToken token = default)
        {
            if (CheckExists(lookup, out TValue performance))
                return performance;

            var computed = await ComputeValueAsync(lookup, token);

            if (computed != null || CacheNullValues)
                cache[lookup] = computed;

            return computed;
        }

        protected bool CheckExists([NotNull] TLookup lookup, out TValue value) =>
            cache.TryGetValue(lookup, out value);

        /// <summary>
        /// Called on cache miss to compute the value for the specified lookup.
        /// </summary>
        /// <param name="lookup">The lookup to retrieve.</param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>The computed value.</returns>
        protected abstract Task<TValue> ComputeValueAsync(TLookup lookup, CancellationToken token = default);
    }
}
