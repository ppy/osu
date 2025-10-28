// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Timing;
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

            // FIRST_LAZER_VERSION is specified here to avoid flooring object coordinates on decode via `(int)` casts.
            // we will be making integers out of them lower down, but in a slightly different manner (rounding rather than truncating)
            var beatmapContent = new LegacyBeatmapDecoder(LegacyBeatmapEncoder.FIRST_LAZER_VERSION).Decode(contentStreamReader);

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

            MutateBeatmap(model, playableBeatmap);

            // Encode to legacy format
            var stream = new MemoryStream();
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                new LegacyBeatmapEncoder(playableBeatmap, beatmapSkin).Encode(sw);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        protected virtual void MutateBeatmap(BeatmapSetInfo beatmapSet, IBeatmap playableBeatmap)
        {
            // Convert beatmap elements to be compatible with legacy format
            // So we truncate time and position values to integers, and convert paths with multiple segments to Bézier curves

            // We must first truncate all timing points and move all objects in the timing section with it to ensure everything stays snapped
            for (int i = 0; i < playableBeatmap.ControlPointInfo.TimingPoints.Count; i++)
            {
                var timingPoint = playableBeatmap.ControlPointInfo.TimingPoints[i];
                double offset = Math.Floor(timingPoint.Time) - timingPoint.Time;
                double nextTimingPointTime = i + 1 < playableBeatmap.ControlPointInfo.TimingPoints.Count
                    ? playableBeatmap.ControlPointInfo.TimingPoints[i + 1].Time
                    : double.PositiveInfinity;

                // Offset all control points in the timing section (including the current one)
                foreach (var controlPoint in playableBeatmap.ControlPointInfo.AllControlPoints.Where(o => o.Time >= timingPoint.Time && o.Time < nextTimingPointTime))
                    controlPoint.Time += offset;

                // Offset all hit objects in the timing section
                foreach (var hitObject in playableBeatmap.HitObjects.Where(o => o.StartTime >= timingPoint.Time && o.StartTime < nextTimingPointTime))
                    hitObject.StartTime += offset;
            }

            foreach (var controlPoint in playableBeatmap.ControlPointInfo.AllControlPoints)
                controlPoint.Time = Math.Floor(controlPoint.Time);

            for (int i = 0; i < playableBeatmap.Breaks.Count; i++)
                playableBeatmap.Breaks[i] = new BreakPeriod(Math.Floor(playableBeatmap.Breaks[i].StartTime), Math.Floor(playableBeatmap.Breaks[i].EndTime));

            foreach (var hitObject in playableBeatmap.HitObjects)
            {
                // Truncate end time before truncating start time because end time is dependent on start time
                if (hitObject is IHasDuration hasDuration && hitObject is not IHasPath)
                    hasDuration.Duration = Math.Floor(hasDuration.EndTime) - Math.Floor(hitObject.StartTime);

                hitObject.StartTime = Math.Floor(hitObject.StartTime);

                if (hitObject is IHasXPosition hasXPosition)
                    hasXPosition.X = MathF.Round(hasXPosition.X);

                if (hitObject is IHasYPosition hasYPosition)
                    hasYPosition.Y = MathF.Round(hasYPosition.Y);

                if (hitObject is not IHasPath hasPath) continue;

                // stable's hit object parsing expects the entire slider to use only one type of curve,
                // and happens to use the last non-empty curve type read for the entire slider.
                // this clear of the last control point type handles an edge case
                // wherein the last control point of an otherwise-single-segment slider path has a different type than previous,
                // which would lead to sliders being mangled when exported back to stable.
                // normally, that would be handled by the `BezierConverter.ConvertToModernBezier()` call below,
                // which outputs a slider path containing only BEZIER control points,
                // but a non-inherited last control point is (rightly) not considered to be starting a new segment,
                // therefore it would fail to clear the `CountSegments() <= 1` check.
                // by clearing explicitly we both fix the issue and avoid unnecessary conversions to BEZIER.
                if (hasPath.Path.ControlPoints.Count > 1)
                    hasPath.Path.ControlPoints[^1].Type = null;

                if (BezierConverter.CountSegments(hasPath.Path.ControlPoints) <= 1
                    && hasPath.Path.ControlPoints[0].Type!.Value.Degree == null)
                {
                    // Round every control point to integer positions before skipping to the next hit object
                    for (int i = 0; i < hasPath.Path.ControlPoints.Count; i++)
                    {
                        var position = new Vector2(
                            MathF.Round(hasPath.Path.ControlPoints[i].Position.X),
                            MathF.Round(hasPath.Path.ControlPoints[i].Position.Y));

                        hasPath.Path.ControlPoints[i].Position = position;
                    }

                    continue;
                }

                var convertedToBezier = BezierConverter.ConvertToModernBezier(hasPath.Path.ControlPoints);

                hasPath.Path.ControlPoints.Clear();

                for (int i = 0; i < convertedToBezier.Count; i++)
                {
                    var convertedPoint = convertedToBezier[i];

                    // Round control points to integer positions
                    var position = new Vector2(
                        MathF.Round(convertedPoint.Position.X),
                        MathF.Round(convertedPoint.Position.Y));

                    // stable only supports a single curve type specification per slider.
                    // we exploit the fact that the converted-to-Bézier path only has Bézier segments,
                    // and thus we specify the Bézier curve type once ever at the start of the slider.
                    hasPath.Path.ControlPoints.Add(new PathControlPoint(position, i == 0 ? PathType.BEZIER : null));

                    // however, the Bézier path as output by the converter has multiple segments.
                    // `LegacyBeatmapEncoder` will attempt to encode this by emitting per-control-point curve type specs which don't do anything for stable.
                    // instead, stable expects control points that start a segment to be present in the path twice in succession.
                    if (convertedPoint.Type == PathType.BEZIER && i > 0)
                        hasPath.Path.ControlPoints.Add(new PathControlPoint(position));
                }
            }
        }

        protected override string FileExtension => @".osz";
    }
}
