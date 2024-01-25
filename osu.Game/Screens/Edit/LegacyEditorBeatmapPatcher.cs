// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.Model;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Types;
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
            IBeatmap newBeatmap = null;

            editorBeatmap.BeginChange();
            processHitObjects(result, () => newBeatmap ??= readBeatmap(newState));
            processTimingPoints(() => newBeatmap ??= readBeatmap(newState));
            processHitObjectLocalData(() => newBeatmap ??= readBeatmap(newState));
            editorBeatmap.EndChange();
        }

        private void processTimingPoints(Func<IBeatmap> getNewBeatmap)
        {
            ControlPointInfo newControlPoints = EditorBeatmap.ConvertControlPoints(getNewBeatmap().ControlPointInfo);

            // Remove all groups from the current beatmap which don't have a corresponding equal group in the new beatmap.
            foreach (var oldGroup in editorBeatmap.ControlPointInfo.Groups.ToArray())
            {
                var newGroup = newControlPoints.GroupAt(oldGroup.Time);

                if (!oldGroup.Equals(newGroup))
                    editorBeatmap.ControlPointInfo.RemoveGroup(oldGroup);
            }

            // Add all groups from the new beatmap which don't have a corresponding equal group in the old beatmap.
            foreach (var newGroup in newControlPoints.Groups)
            {
                var oldGroup = editorBeatmap.ControlPointInfo.GroupAt(newGroup.Time);

                if (!newGroup.Equals(oldGroup))
                {
                    foreach (var point in newGroup.ControlPoints)
                        editorBeatmap.ControlPointInfo.Add(newGroup.Time, point);
                }
            }
        }

        private void processHitObjects(DiffResult result, Func<IBeatmap> getNewBeatmap)
        {
            findChangedIndices(result, LegacyDecoder<Beatmap>.Section.HitObjects, out var removedIndices, out var addedIndices);

            for (int i = removedIndices.Count - 1; i >= 0; i--)
                editorBeatmap.RemoveAt(removedIndices[i]);

            if (addedIndices.Count > 0)
            {
                var newBeatmap = getNewBeatmap();

                foreach (int i in addedIndices)
                    editorBeatmap.Insert(i, newBeatmap.HitObjects[i]);
            }
        }

        private void processHitObjectLocalData(Func<IBeatmap> getNewBeatmap)
        {
            // This method handles data that are stored in control points in the legacy format,
            // but were moved to the hitobjects themselves in lazer.
            // Specifically, the data being referred to here consists of: slider velocity and sample information.

            // For simplicity, this implementation relies on the editor beatmap already having the same hitobjects in sequence as the new beatmap.
            // To guarantee that, `processHitObjects()` must be ran prior to this method for correct operation.
            // This is done to avoid the necessity of reimplementing/reusing parts of LegacyBeatmapDecoder that already treat this data correctly.

            var oldObjects = editorBeatmap.HitObjects;
            var newObjects = getNewBeatmap().HitObjects;

            Debug.Assert(oldObjects.Count == newObjects.Count);

            foreach (var (oldObject, newObject) in oldObjects.Zip(newObjects))
            {
                // if `oldObject` and `newObject` are the same, it means that `oldObject` was inserted into `editorBeatmap` by `processHitObjects()`.
                // in that case, there is nothing to do (and some of the subsequent changes may even prove destructive).
                if (ReferenceEquals(oldObject, newObject))
                    continue;

                if (oldObject is IHasSliderVelocity oldWithVelocity && newObject is IHasSliderVelocity newWithVelocity)
                    oldWithVelocity.SliderVelocityMultiplier = newWithVelocity.SliderVelocityMultiplier;

                oldObject.Samples = newObject.Samples;

                if (oldObject is IHasRepeats oldWithRepeats && newObject is IHasRepeats newWithRepeats)
                {
                    oldWithRepeats.NodeSamples.Clear();
                    oldWithRepeats.NodeSamples.AddRange(newWithRepeats.NodeSamples);
                }

                editorBeatmap.Update(oldObject);
            }
        }

        private void findChangedIndices(DiffResult result, LegacyDecoder<Beatmap>.Section section, out List<int> removedIndices, out List<int> addedIndices)
        {
            removedIndices = new List<int>();
            addedIndices = new List<int>();

            // Find the start and end indices of the relevant section headers in both the old and the new beatmap file. Lines changed outside of the modified ranges are ignored.
            int oldSectionStartIndex = Array.IndexOf(result.PiecesOld, $"[{section}]");
            if (oldSectionStartIndex == -1)
                return;

            int oldSectionEndIndex = Array.FindIndex(result.PiecesOld, oldSectionStartIndex + 1, s => s.StartsWith('['));
            if (oldSectionEndIndex == -1)
                oldSectionEndIndex = result.PiecesOld.Length;

            int newSectionStartIndex = Array.IndexOf(result.PiecesNew, $"[{section}]");
            if (newSectionStartIndex == -1)
                return;

            int newSectionEndIndex = Array.FindIndex(result.PiecesNew, newSectionStartIndex + 1, s => s.StartsWith('['));
            if (newSectionEndIndex == -1)
                newSectionEndIndex = result.PiecesNew.Length;

            foreach (var block in result.DiffBlocks)
            {
                // Removed indices
                for (int i = 0; i < block.DeleteCountA; i++)
                {
                    int objectIndex = block.DeleteStartA + i;

                    if (objectIndex <= oldSectionStartIndex || objectIndex >= oldSectionEndIndex)
                        continue;

                    removedIndices.Add(objectIndex - oldSectionStartIndex - 1);
                }

                // Added indices
                for (int i = 0; i < block.InsertCountB; i++)
                {
                    int objectIndex = block.InsertStartB + i;

                    if (objectIndex <= newSectionStartIndex || objectIndex >= newSectionEndIndex)
                        continue;

                    addedIndices.Add(objectIndex - newSectionStartIndex - 1);
                }
            }

            // Sort the indices to ensure that removal + insertion indices don't get jumbled up post-removal or post-insertion.
            // This isn't strictly required, but the differ makes no guarantees about order.
            removedIndices.Sort();
            addedIndices.Sort();
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

            public override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected internal override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }
    }
}
