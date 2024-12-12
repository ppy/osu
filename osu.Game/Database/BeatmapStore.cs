// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Database
{
    /// <summary>
    /// A store which contains a thread-safe representation of beatmaps available game-wide.
    /// This exposes changes to available beatmaps, such as post-import or deletion.
    /// </summary>
    /// <remarks>
    /// The main goal of classes which implement this interface should be to provide change
    /// tracking and thread safety in a performant way, rather than having to worry about such
    /// concerns at the point of usage.
    /// </remarks>
    public abstract partial class BeatmapStore : Component
    {
        /// <summary>
        /// Get all available beatmaps.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token which allows early abort from the operation.</param>
        /// <returns>A bindable list of all available beatmap sets.</returns>
        /// <remarks>
        /// This operation may block during the initial load process.
        ///
        /// It is generally expected that once a beatmap store is in a good state, the overhead of this call
        /// should be negligible.
        /// </remarks>
        public abstract IBindableList<BeatmapSetInfo> GetBeatmapSets(CancellationToken? cancellationToken);
    }
}
