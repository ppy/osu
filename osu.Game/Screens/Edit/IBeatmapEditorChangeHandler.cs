// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Interface for a component that manages changes in the <see cref="EditorBeatmap"/>.
    /// </summary>
    [Cached]
    public interface IBeatmapEditorChangeHandler : IEditorChangeHandler
    {
        /// <summary>
        /// Records to the history that a <see cref="HitObject"/> has been changed. This makes sure hit objects are properly updated on undo/redo operations.
        /// </summary>
        /// <param name="hitObject">Hit object which was changed.</param>
        void Update(HitObject hitObject);
    }
}
