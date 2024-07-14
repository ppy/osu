// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Text;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    public partial class BeatmapEditorChangeHandler : EditorChangeHandler
    {
        private readonly LegacyEditorBeatmapPatcher patcher;
        private readonly EditorBeatmap editorBeatmap;

        /// <summary>
        /// Creates a new <see cref="EditorChangeHandler"/>.
        /// </summary>
        /// <param name="editorBeatmap">The <see cref="EditorBeatmap"/> to track the <see cref="HitObject"/>s of.</param>
        public BeatmapEditorChangeHandler(EditorBeatmap editorBeatmap)
        {
            this.editorBeatmap = editorBeatmap;

            editorBeatmap.TransactionBegan += BeginChange;
            editorBeatmap.TransactionEnded += EndChange;
            editorBeatmap.SaveStateTriggered += SaveState;

            patcher = new LegacyEditorBeatmapPatcher(editorBeatmap);
        }

        protected override void WriteCurrentStateToStream(MemoryStream stream)
        {
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new LegacyBeatmapEncoder(editorBeatmap, editorBeatmap.BeatmapSkin).Encode(sw);
        }

        protected override void ApplyStateChange(byte[] previousState, byte[] newState) =>
            patcher.Patch(previousState, newState);
    }
}
