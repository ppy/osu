// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Interface for the <see cref="IBeatmap"/> contained by the see <see cref="HitObjectComposer"/>.
    /// Children of <see cref="HitObjectComposer"/> may resolve the beatmap via <see cref="IEditorBeatmap"/> or <see cref="IEditorBeatmap{T}"/>.
    /// </summary>
    public interface IEditorBeatmap : IBeatmap
    {
        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is added to this <see cref="IEditorBeatmap"/>.
        /// </summary>
        event Action<HitObject> HitObjectAdded;

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is removed from this <see cref="IEditorBeatmap"/>.
        /// </summary>
        event Action<HitObject> HitObjectRemoved;
    }

    /// <summary>
    /// Interface for the <see cref="IBeatmap"/> contained by the see <see cref="HitObjectComposer"/>.
    /// Children of <see cref="HitObjectComposer"/> may resolve the beatmap via <see cref="IEditorBeatmap"/> or <see cref="IEditorBeatmap{T}"/>.
    /// </summary>
    public interface IEditorBeatmap<out T> : IEditorBeatmap, IBeatmap<T>
        where T : HitObject
    {
    }
}
