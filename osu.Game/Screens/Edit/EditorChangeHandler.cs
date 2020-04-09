// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Tracks changes to the <see cref="Editor"/>.
    /// </summary>
    public class EditorChangeHandler : IEditorChangeHandler
    {
        private readonly LegacyEditorBeatmapDiffer differ;
        private readonly List<Stream> savedStates = new List<Stream>();
        private int currentState = -1;

        private readonly EditorBeatmap editorBeatmap;
        private int bulkChangesStarted;
        private bool isRestoring;

        /// <summary>
        /// Creates a new <see cref="EditorChangeHandler"/>.
        /// </summary>
        /// <param name="editorBeatmap">The <see cref="EditorBeatmap"/> to track the <see cref="HitObject"/>s of.</param>
        public EditorChangeHandler(EditorBeatmap editorBeatmap)
        {
            this.editorBeatmap = editorBeatmap;

            editorBeatmap.HitObjectAdded += hitObjectAdded;
            editorBeatmap.HitObjectRemoved += hitObjectRemoved;
            editorBeatmap.HitObjectUpdated += hitObjectUpdated;

            differ = new LegacyEditorBeatmapDiffer(editorBeatmap);

            // Initial state.
            SaveState();
        }

        private void hitObjectAdded(HitObject obj) => SaveState();

        private void hitObjectRemoved(HitObject obj) => SaveState();

        private void hitObjectUpdated(HitObject obj) => SaveState();

        public void BeginChange() => bulkChangesStarted++;

        public void EndChange()
        {
            if (bulkChangesStarted == 0)
                throw new InvalidOperationException($"Cannot call {nameof(EndChange)} without a previous call to {nameof(BeginChange)}.");

            if (--bulkChangesStarted == 0)
                SaveState();
        }

        /// <summary>
        /// Saves the current <see cref="Editor"/> state.
        /// </summary>
        public void SaveState()
        {
            if (bulkChangesStarted > 0)
                return;

            if (isRestoring)
                return;

            var stream = new MemoryStream();

            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new LegacyBeatmapEncoder(editorBeatmap).Encode(sw);

            if (currentState < savedStates.Count - 1)
                savedStates.RemoveRange(currentState + 1, savedStates.Count - currentState - 1);

            savedStates.Add(stream);
            currentState = savedStates.Count - 1;
        }

        /// <summary>
        /// Restores an older or newer state.
        /// </summary>
        /// <param name="direction">The direction to restore in. If less than 0, an older state will be used. If greater than 0, a newer state will be used.</param>
        public void RestoreState(int direction)
        {
            if (bulkChangesStarted > 0)
                return;

            if (savedStates.Count == 0)
                return;

            int newState = Math.Clamp(currentState + direction, 0, savedStates.Count - 1);
            if (currentState == newState)
                return;

            isRestoring = true;

            differ.Patch(savedStates[currentState], savedStates[newState]);
            currentState = newState;

            isRestoring = false;
        }
    }
}
