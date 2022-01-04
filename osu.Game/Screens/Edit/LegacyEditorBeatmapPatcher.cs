// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using DiffPlex;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Skinning;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Patches an <see cref="EditorBeatmap"/> based on the difference between two legacy (.osu) states.
    /// </summary>
    public class LegacyEditorBeatmapPatcher
    {
        private readonly EditorBeatmap editorBeatmap;

        public LegacyEditorBeatmapPatcher(EditorBeatmap editorBeatmap)
        {
            this.editorBeatmap = editorBeatmap;
        }

        public void Patch(byte[] currentState, byte[] newState)
        {
            // Diff the beatmaps
            var result = new Differ().CreateLineDiffs(readString(currentState), readString(newState), true, false);

            // Find the index of [HitObject] sections. Lines changed prior to this index are ignored.
            int oldHitObjectsIndex = Array.IndexOf(result.PiecesOld, "[HitObjects]");
            int newHitObjectsIndex = Array.IndexOf(result.PiecesNew, "[HitObjects]");

            Debug.Assert(oldHitObjectsIndex >= 0);
            Debug.Assert(newHitObjectsIndex >= 0);

            var toRemove = new List<int>();
            var toAdd = new List<int>();

            foreach (var block in result.DiffBlocks)
            {
                // Removed hitobjects
                for (int i = 0; i < block.DeleteCountA; i++)
                {
                    int hoIndex = block.DeleteStartA + i - oldHitObjectsIndex - 1;

                    if (hoIndex < 0)
                        continue;

                    toRemove.Add(hoIndex);
                }

                // Added hitobjects
                for (int i = 0; i < block.InsertCountB; i++)
                {
                    int hoIndex = block.InsertStartB + i - newHitObjectsIndex - 1;

                    if (hoIndex < 0)
                        continue;

                    toAdd.Add(hoIndex);
                }
            }

            // Sort the indices to ensure that removal + insertion indices don't get jumbled up post-removal or post-insertion.
            // This isn't strictly required, but the differ makes no guarantees about order.
            toRemove.Sort();
            toAdd.Sort();

            editorBeatmap.BeginChange();

            // Apply the changes.
            for (int i = toRemove.Count - 1; i >= 0; i--)
                editorBeatmap.RemoveAt(toRemove[i]);

            if (toAdd.Count > 0)
            {
                IBeatmap newBeatmap = readBeatmap(newState);
                foreach (int i in toAdd)
                    editorBeatmap.Insert(i, newBeatmap.HitObjects[i]);
            }

            editorBeatmap.EndChange();
        }

        private string readString(byte[] state) => Encoding.UTF8.GetString(state);

        private IBeatmap readBeatmap(byte[] state)
        {
            using (var stream = new MemoryStream(state))
            using (var reader = new LineBufferedReader(stream, true))
            {
                var decoded = Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
                decoded.BeatmapInfo.Ruleset = editorBeatmap.BeatmapInfo.Ruleset;
                return new PassThroughWorkingBeatmap(decoded).GetPlayableBeatmap(editorBeatmap.BeatmapInfo.Ruleset);
            }
        }

        private class PassThroughWorkingBeatmap : WorkingBeatmap
        {
            private readonly IBeatmap beatmap;

            public PassThroughWorkingBeatmap(IBeatmap beatmap)
                : base(beatmap.BeatmapInfo, null)
            {
                this.beatmap = beatmap;
            }

            protected override IBeatmap GetBeatmap() => beatmap;

            protected override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
