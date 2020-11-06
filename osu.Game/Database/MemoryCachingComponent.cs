// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using osu.Framework.Graphics;

namespace osu.Game.Database
{
    /// <summary>
    /// A component which performs lookups (or calculations) and caches the results.
    /// Currently not persisted between game sessions.
    /// </summary>
    public abstract class MemoryCachingComponent<TLookup, TValue> : Component
    {
        protected readonly ConcurrentDictionary<TLookup, TValue> Cache = new ConcurrentDictionary<TLookup, TValue>();
    }
}
