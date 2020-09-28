// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Tracks changes to the <see cref="Editor"/>.
    /// </summary>
    public class EditorChangeHandler : IEditorChangeHandler
    {
        public readonly Bindable<bool> CanUndo = new Bindable<bool>();
        public readonly Bindable<bool> CanRedo = new Bindable<bool>();

        private readonly LegacyEditorBeatmapPatcher patcher;
        private readonly List<byte[]> savedStates = new List<byte[]>();

        private int currentState = -1;

        /// <summary>
        /// A SHA-2 hash representing the current visible editor state.
        /// </summary>
        public string CurrentStateHash
        {
            get
            {
                using (var stream = new MemoryStream(savedStates[currentState]))
                    return stream.ComputeSHA2Hash();
            }
        }

        private readonly EditorBeatmap editorBeatmap;
        private int bulkChangesStarted;
        private bool isRestoring;

        public const int MAX_SAVED_STATES = 50;

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

            patcher = new LegacyEditorBeatmapPatcher(editorBeatmap);

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

            using (var stream = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    new LegacyBeatmapEncoder(editorBeatmap, editorBeatmap.BeatmapSkin).Encode(sw);

                var newState = stream.ToArray();

                // if the previous state is binary equal we don't need to push a new one, unless this is the initial state.
                if (savedStates.Count > 0 && newState.SequenceEqual(savedStates.Last())) return;

                if (currentState < savedStates.Count - 1)
                    savedStates.RemoveRange(currentState + 1, savedStates.Count - currentState - 1);

                if (savedStates.Count > MAX_SAVED_STATES)
                    savedStates.RemoveAt(0);

                savedStates.Add(newState);

                currentState = savedStates.Count - 1;
                updateBindables();
            }
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

            patcher.Patch(savedStates[currentState], savedStates[newState]);
            currentState = newState;

            isRestoring = false;

            updateBindables();
        }

        private void updateBindables()
        {
            CanUndo.Value = savedStates.Count > 0 && currentState > 0;
            CanRedo.Value = currentState < savedStates.Count - 1;
        }
    }
}
