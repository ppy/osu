// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using DiffPlex;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;

namespace osu.Game.Screens.Edit
{
    public class LegacyEditorBeatmapDiffer
    {
        private readonly EditorBeatmap editorBeatmap;

        public LegacyEditorBeatmapDiffer(EditorBeatmap editorBeatmap)
        {
            this.editorBeatmap = editorBeatmap;
        }

        public void Patch(Stream currentState, Stream newState)
        {
            // Diff the beatmaps
            var result = new Differ().CreateLineDiffs(readString(currentState), readString(newState), true, false);

            // Find the index of [HitObject] sections. Lines changed prior to this index are ignored.
            int oldHitObjectsIndex = Array.IndexOf(result.PiecesOld, "[HitObjects]");
            int newHitObjectsIndex = Array.IndexOf(result.PiecesNew, "[HitObjects]");

            var toRemove = new List<int>();
            var toAdd = new List<int>();

            foreach (var block in result.DiffBlocks)
            {
                // Removed hitobject
                for (int i = 0; i < block.DeleteCountA; i++)
                {
                    int hoIndex = block.DeleteStartA + i - oldHitObjectsIndex - 1;

                    if (hoIndex < 0)
                        continue;

                    toRemove.Add(hoIndex);
                }

                // Added hitobject
                for (int i = 0; i < block.InsertCountB; i++)
                {
                    int hoIndex = block.InsertStartB + i - newHitObjectsIndex - 1;

                    if (hoIndex < 0)
                        continue;

                    toAdd.Add(hoIndex);
                }
            }

            // Make the removal indices are sorted so that iteration order doesn't get messed up post-removal.
            toRemove.Sort();

            // Apply the changes.
            for (int i = toRemove.Count - 1; i >= 0; i--)
                editorBeatmap.RemoveAt(toRemove[i]);

            if (toAdd.Count > 0)
            {
                IBeatmap newBeatmap = readBeatmap(newState);
                foreach (var i in toAdd)
                    editorBeatmap.Add(newBeatmap.HitObjects[i]);
            }
        }

        private string readString(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var sr = new StreamReader(stream, System.Text.Encoding.UTF8, true, 1024, true))
                return sr.ReadToEnd();
        }

        private IBeatmap readBeatmap(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var reader = new LineBufferedReader(stream, true))
                return new PassThroughWorkingBeatmap(Decoder.GetDecoder<Beatmap>(reader).Decode(reader)).GetPlayableBeatmap(editorBeatmap.BeatmapInfo.Ruleset);
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

            protected override Track GetTrack() => throw new NotImplementedException();
        }
    }
}
