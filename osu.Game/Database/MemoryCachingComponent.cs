// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Statistics;

namespace osu.Game.Database
{
    /// <summary>
    /// A component which performs lookups (or calculations) and caches the results.
    /// Currently not persisted between game sessions.
    /// </summary>
    public abstract partial class MemoryCachingComponent<TLookup, TValue> : Component
        where TLookup : notnull
    {
        private readonly ConcurrentDictionary<TLookup, TValue?> cache = new ConcurrentDictionary<TLookup, TValue?>();

        private readonly GlobalStatistic<MemoryCachingStatistics> statistics;

        protected virtual bool CacheNullValues => true;

        protected MemoryCachingComponent()
        {
            statistics = GlobalStatistics.Get<MemoryCachingStatistics>(nameof(MemoryCachingComponent<TLookup, TValue>), GetType().ReadableName());
            statistics.Value = new MemoryCachingStatistics();
        }

        /// <summary>
        /// Retrieve the cached value for the given lookup.
        /// </summary>
        /// <param name="lookup">The lookup to retrieve.</param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        protected async Task<TValue?> GetAsync(TLookup lookup, CancellationToken token = default)
        {
            if (CheckExists(lookup, out TValue? existing))
            {
                statistics.Value.HitCount++;
                return existing;
            }

            var computed = await ComputeValueAsync(lookup, token).ConfigureAwait(false);

            statistics.Value.MissCount++;

            if (computed != null || CacheNullValues)
            {
                cache[lookup] = computed;
                statistics.Value.Usage = cache.Count;
            }

            return computed;
        }

        /// <summary>
        /// Invalidate all entries matching a provided predicate.
        /// </summary>
        /// <param name="matchKeyPredicate">The predicate to decide which keys should be invalidated.</param>
        protected void Invalidate(Func<TLookup, bool> matchKeyPredicate)
        {
            foreach (var kvp in cache)
            {
                if (matchKeyPredicate(kvp.Key))
                    cache.TryRemove(kvp.Key, out _);
            }

            statistics.Value.Usage = cache.Count;
        }

        protected bool CheckExists(TLookup lookup, [MaybeNullWhen(false)] out TValue value) =>
            cache.TryGetValue(lookup, out value);

        /// <summary>
        /// Called on cache miss to compute the value for the specified lookup.
        /// </summary>
        /// <param name="lookup">The lookup to retrieve.</param>
        /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>The computed value.</returns>
        protected abstract Task<TValue?> ComputeValueAsync(TLookup lookup, CancellationToken token = default);

        private class MemoryCachingStatistics
        {
            /// <summary>
            /// Total number of cache hits.
            /// </summary>
            public int HitCount;

            /// <summary>
            /// Total number of cache misses.
            /// </summary>
            public int MissCount;

            /// <summary>
            /// Total number of cached entities.
            /// </summary>
            public int Usage;

            public override string ToString()
            {
                int totalAccesses = HitCount + MissCount;
                double hitRate = totalAccesses == 0 ? 0 : (double)HitCount / totalAccesses;

                return $"i:{Usage} h:{HitCount} m:{MissCount} {hitRate:0%}";
            }
        }
    }
}
