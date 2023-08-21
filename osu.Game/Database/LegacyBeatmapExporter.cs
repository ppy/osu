// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Database
{
    /// <summary>
    /// Exporter for osu!stable legacy beatmap archives.
    /// Converts all beatmaps in the set to legacy format and exports it as a legacy package.
    /// </summary>
    public class LegacyBeatmapExporter : LegacyArchiveExporter<BeatmapSetInfo>
    {
        public LegacyBeatmapExporter(Storage storage)
            : base(storage)
        {
        }

        protected override Stream? GetFileContents(BeatmapSetInfo model, INamedFileUsage file)
        {
            var beatmapInfo = model.Beatmaps.SingleOrDefault(o => o.Hash == file.File.Hash);

            if (beatmapInfo == null)
                return base.GetFileContents(model, file);

            // Read the beatmap contents and skin
            using var contentStream = base.GetFileContents(model, file);

            if (contentStream == null)
                return null;

            using var contentStreamReader = new LineBufferedReader(contentStream);
            var beatmapContent = new LegacyBeatmapDecoder().Decode(contentStreamReader);

            var workingBeatmap = new FlatWorkingBeatmap(beatmapContent);
            var playableBeatmap = workingBeatmap.GetPlayableBeatmap(beatmapInfo.Ruleset);

            using var skinStream = base.GetFileContents(model, file);

            if (skinStream == null)
                return null;

            using var skinStreamReader = new LineBufferedReader(skinStream);
            var beatmapSkin = new LegacySkin(new SkinInfo(), null!)
            {
                Configuration = new LegacySkinDecoder().Decode(skinStreamReader)
            };

            // Convert beatmap elements to be compatible with legacy format
            // So we truncate time and position values to integers, and convert paths with multiple segments to bezier curves
            foreach (var controlPoint in playableBeatmap.ControlPointInfo.AllControlPoints)
                controlPoint.Time = Math.Floor(controlPoint.Time);

            foreach (var hitObject in playableBeatmap.HitObjects)
            {
                // Truncate end time before truncating start time because end time is dependent on start time
                if (hitObject is IHasDuration hasDuration && hitObject is not IHasPath)
                    hasDuration.Duration = Math.Floor(hasDuration.EndTime) - Math.Floor(hitObject.StartTime);

                hitObject.StartTime = Math.Floor(hitObject.StartTime);

                if (hitObject is not IHasPath hasPath) continue;

                // stable's hit object parsing expects the entire slider to use only one type of curve,
                // and happens to use the last non-empty curve type read for the entire slider.
                // this clear of the last control point type handles an edge case
                // wherein the last control point of an otherwise-single-segment slider path has a different type than previous,
                // which would lead to sliders being mangled when exported back to stable.
                // normally, that would be handled by the `BezierConverter.ConvertToModernBezier()` call below,
                // which outputs a slider path containing only Bezier control points,
                // but a non-inherited last control point is (rightly) not considered to be starting a new segment,
                // therefore it would fail to clear the `CountSegments() <= 1` check.
                // by clearing explicitly we both fix the issue and avoid unnecessary conversions to Bezier.
                if (hasPath.Path.ControlPoints.Count > 1)
                    hasPath.Path.ControlPoints[^1].Type = null;

                if (BezierConverter.CountSegments(hasPath.Path.ControlPoints) <= 1) continue;

                var newControlPoints = BezierConverter.ConvertToModernBezier(hasPath.Path.ControlPoints);

                // Truncate control points to integer positions
                foreach (var pathControlPoint in newControlPoints)
                {
                    pathControlPoint.Position = new Vector2(
                        (float)Math.Floor(pathControlPoint.Position.X),
                        (float)Math.Floor(pathControlPoint.Position.Y));
                }

                hasPath.Path.ControlPoints.Clear();
                hasPath.Path.ControlPoints.AddRange(newControlPoints);
            }

            // Encode to legacy format
            var stream = new MemoryStream();
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new LegacyBeatmapEncoder(playableBeatmap, beatmapSkin).Encode(sw);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        protected override string FileExtension => @".osz";
    }
}
