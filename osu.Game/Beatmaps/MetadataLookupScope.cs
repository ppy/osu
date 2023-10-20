// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Determines which sources (if any at all) should be queried in which order for a beatmap's metadata.
    /// </summary>
    public enum MetadataLookupScope
    {
        /// <summary>
        /// Do not attempt to look up the beatmap metadata either in the local cache or online.
        /// </summary>
        None,

        /// <summary>
        /// Try the local metadata cache first before querying online sources.
        /// </summary>
        LocalCacheFirst,

        /// <summary>
        /// Query online sources immediately.
        /// </summary>
        OnlineFirst
    }
}
