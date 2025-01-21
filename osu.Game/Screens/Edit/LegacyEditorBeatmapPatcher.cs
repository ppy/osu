// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
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
            IBeatmap newBeatmap = null;

            editorBeatmap.BeginChange();
            processTimingPoints(() => newBeatmap ??= readBeatmap(newState));
            processBreaks(() => newBeatmap ??= readBeatmap(newState));
            processBookmarks(() => newBeatmap ??= readBeatmap(newState));
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

        private void processBreaks(Func<IBeatmap> getNewBeatmap)
        {
            var newBreaks = getNewBeatmap().Breaks.ToArray();

            foreach (var oldBreak in editorBeatmap.Breaks.ToArray())
            {
                if (newBreaks.Any(b => b.Equals(oldBreak)))
                    continue;

                editorBeatmap.Breaks.Remove(oldBreak);
            }

            foreach (var newBreak in newBreaks)
            {
                if (editorBeatmap.Breaks.Any(b => b.Equals(newBreak)))
                    continue;

                editorBeatmap.Breaks.Add(newBreak);
            }
        }

        private void processBookmarks(Func<IBeatmap> getNewBeatmap)
        {
            var newBookmarks = getNewBeatmap().Bookmarks.ToHashSet();

            foreach (int oldBookmark in editorBeatmap.Bookmarks.ToArray())
            {
                if (newBookmarks.Contains(oldBookmark))
                    continue;

                editorBeatmap.Bookmarks.Remove(oldBookmark);
            }

            foreach (int newBookmark in newBookmarks)
            {
                if (editorBeatmap.Bookmarks.Contains(newBookmark))
                    continue;

                editorBeatmap.Bookmarks.Add(newBookmark);
            }
        }

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
