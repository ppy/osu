// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapEncoder
    {
        public const int LATEST_VERSION = 128;

        /// <summary>
        /// osu! is generally slower than taiko, so a factor is added to increase
        /// speed. This must be used everywhere slider length or beat length is used.
        /// </summary>
        public const float LEGACY_TAIKO_VELOCITY_MULTIPLIER = 1.4f;

        private readonly IBeatmap beatmap;

        [CanBeNull]
        private readonly ISkin skin;

        /// <summary>
        /// Creates a new <see cref="LegacyBeatmapEncoder"/>.
        /// </summary>
        /// <param name="beatmap">The beatmap to encode.</param>
        /// <param name="skin">The beatmap's skin, used for encoding combo colours.</param>
        public LegacyBeatmapEncoder(IBeatmap beatmap, [CanBeNull] ISkin skin)
        {
            this.beatmap = beatmap;
            this.skin = skin;

            if (beatmap.BeatmapInfo.RulesetID < 0 || beatmap.BeatmapInfo.RulesetID > 3)
                throw new ArgumentException("Only beatmaps in the osu, taiko, catch, or mania rulesets can be encoded to the legacy beatmap format.", nameof(beatmap));
        }

        public void Encode(TextWriter writer)
        {
            writer.WriteLine($"osu file format v{LATEST_VERSION}");

            writer.WriteLine();
            handleGeneral(writer);

            writer.WriteLine();
            handleEditor(writer);

            writer.WriteLine();
            handleMetadata(writer);

            writer.WriteLine();
            handleDifficulty(writer);

            writer.WriteLine();
            handleEvents(writer);

            writer.WriteLine();
            handleControlPoints(writer);

            writer.WriteLine();
            handleColours(writer);

            writer.WriteLine();
            handleHitObjects(writer);
        }

        private void handleGeneral(TextWriter writer)
        {
            writer.WriteLine("[General]");

            if (!string.IsNullOrEmpty(beatmap.Metadata.AudioFile)) writer.WriteLine(FormattableString.Invariant($"AudioFilename: {Path.GetFileName(beatmap.Metadata.AudioFile)}"));
            writer.WriteLine(FormattableString.Invariant($"AudioLeadIn: {beatmap.BeatmapInfo.AudioLeadIn}"));
            writer.WriteLine(FormattableString.Invariant($"PreviewTime: {beatmap.Metadata.PreviewTime}"));
            writer.WriteLine(FormattableString.Invariant($"Countdown: {(int)beatmap.BeatmapInfo.Countdown}"));
            writer.WriteLine(FormattableString.Invariant($"SampleSet: {toLegacySampleBank((beatmap.HitObjects.FirstOrDefault()?.SampleControlPoint ?? SampleControlPoint.DEFAULT).SampleBank)}"));
            writer.WriteLine(FormattableString.Invariant($"StackLeniency: {beatmap.BeatmapInfo.StackLeniency}"));
            writer.WriteLine(FormattableString.Invariant($"Mode: {beatmap.BeatmapInfo.RulesetID}"));
            writer.WriteLine(FormattableString.Invariant($"LetterboxInBreaks: {(beatmap.BeatmapInfo.LetterboxInBreaks ? '1' : '0')}"));
            // if (beatmap.BeatmapInfo.UseSkinSprites)
            //     writer.WriteLine(@"UseSkinSprites: 1");
            // if (b.AlwaysShowPlayfield)
            //     writer.WriteLine(@"AlwaysShowPlayfield: 1");
            // if (b.OverlayPosition != OverlayPosition.NoChange)
            //     writer.WriteLine(@"OverlayPosition: " + b.OverlayPosition);
            // if (!string.IsNullOrEmpty(b.SkinPreference))
            //     writer.WriteLine(@"SkinPreference:" + b.SkinPreference);
            if (beatmap.BeatmapInfo.EpilepsyWarning)
                writer.WriteLine(@"EpilepsyWarning: 1");
            if (beatmap.BeatmapInfo.CountdownOffset > 0)
                writer.WriteLine(FormattableString.Invariant($@"CountdownOffset: {beatmap.BeatmapInfo.CountdownOffset}"));
            if (beatmap.BeatmapInfo.RulesetID == 3)
                writer.WriteLine(FormattableString.Invariant($"SpecialStyle: {(beatmap.BeatmapInfo.SpecialStyle ? '1' : '0')}"));
            writer.WriteLine(FormattableString.Invariant($"WidescreenStoryboard: {(beatmap.BeatmapInfo.WidescreenStoryboard ? '1' : '0')}"));
            if (beatmap.BeatmapInfo.SamplesMatchPlaybackRate)
                writer.WriteLine(@"SamplesMatchPlaybackRate: 1");
        }

        private void handleEditor(TextWriter writer)
        {
            writer.WriteLine("[Editor]");

            if (beatmap.BeatmapInfo.Bookmarks.Length > 0)
                writer.WriteLine(FormattableString.Invariant($"Bookmarks: {string.Join(',', beatmap.BeatmapInfo.Bookmarks)}"));
            writer.WriteLine(FormattableString.Invariant($"DistanceSpacing: {beatmap.BeatmapInfo.DistanceSpacing}"));
            writer.WriteLine(FormattableString.Invariant($"BeatDivisor: {beatmap.BeatmapInfo.BeatDivisor}"));
            writer.WriteLine(FormattableString.Invariant($"GridSize: {beatmap.BeatmapInfo.GridSize}"));
            writer.WriteLine(FormattableString.Invariant($"TimelineZoom: {beatmap.BeatmapInfo.TimelineZoom}"));
        }

        private void handleMetadata(TextWriter writer)
        {
            writer.WriteLine("[Metadata]");

            writer.WriteLine(FormattableString.Invariant($"Title: {beatmap.Metadata.Title}"));
            if (!string.IsNullOrEmpty(beatmap.Metadata.TitleUnicode)) writer.WriteLine(FormattableString.Invariant($"TitleUnicode: {beatmap.Metadata.TitleUnicode}"));
            writer.WriteLine(FormattableString.Invariant($"Artist: {beatmap.Metadata.Artist}"));
            if (!string.IsNullOrEmpty(beatmap.Metadata.ArtistUnicode)) writer.WriteLine(FormattableString.Invariant($"ArtistUnicode: {beatmap.Metadata.ArtistUnicode}"));
            writer.WriteLine(FormattableString.Invariant($"Creator: {beatmap.Metadata.Author.Username}"));
            writer.WriteLine(FormattableString.Invariant($"Version: {beatmap.BeatmapInfo.DifficultyName}"));
            if (!string.IsNullOrEmpty(beatmap.Metadata.Source)) writer.WriteLine(FormattableString.Invariant($"Source: {beatmap.Metadata.Source}"));
            if (!string.IsNullOrEmpty(beatmap.Metadata.Tags)) writer.WriteLine(FormattableString.Invariant($"Tags: {beatmap.Metadata.Tags}"));
            if (beatmap.BeatmapInfo.OnlineID != null) writer.WriteLine(FormattableString.Invariant($"BeatmapID: {beatmap.BeatmapInfo.OnlineID}"));
            if (beatmap.BeatmapInfo.BeatmapSet?.OnlineID != null) writer.WriteLine(FormattableString.Invariant($"BeatmapSetID: {beatmap.BeatmapInfo.BeatmapSet.OnlineID}"));
        }

        private void handleDifficulty(TextWriter writer)
        {
            writer.WriteLine("[Difficulty]");

            writer.WriteLine(FormattableString.Invariant($"HPDrainRate: {beatmap.Difficulty.DrainRate}"));
            writer.WriteLine(FormattableString.Invariant($"CircleSize: {beatmap.Difficulty.CircleSize}"));
            writer.WriteLine(FormattableString.Invariant($"OverallDifficulty: {beatmap.Difficulty.OverallDifficulty}"));
            writer.WriteLine(FormattableString.Invariant($"ApproachRate: {beatmap.Difficulty.ApproachRate}"));

            // Taiko adjusts the slider multiplier (see: LEGACY_TAIKO_VELOCITY_MULTIPLIER)
            writer.WriteLine(beatmap.BeatmapInfo.RulesetID == 1
                ? FormattableString.Invariant($"SliderMultiplier: {beatmap.Difficulty.SliderMultiplier / LEGACY_TAIKO_VELOCITY_MULTIPLIER}")
                : FormattableString.Invariant($"SliderMultiplier: {beatmap.Difficulty.SliderMultiplier}"));

            writer.WriteLine(FormattableString.Invariant($"SliderTickRate: {beatmap.Difficulty.SliderTickRate}"));
        }

        private void handleEvents(TextWriter writer)
        {
            writer.WriteLine("[Events]");

            if (!string.IsNullOrEmpty(beatmap.BeatmapInfo.Metadata.BackgroundFile))
                writer.WriteLine(FormattableString.Invariant($"{(int)LegacyEventType.Background},0,\"{beatmap.BeatmapInfo.Metadata.BackgroundFile}\",0,0"));

            foreach (var b in beatmap.Breaks)
                writer.WriteLine(FormattableString.Invariant($"{(int)LegacyEventType.Break},{b.StartTime},{b.EndTime}"));
        }

        private void handleControlPoints(TextWriter writer)
        {
            if (beatmap.ControlPointInfo.Groups.Count == 0)
                return;

            var legacyControlPoints = new LegacyControlPointInfo();
            foreach (var point in beatmap.ControlPointInfo.AllControlPoints)
                legacyControlPoints.Add(point.Time, point.DeepClone());

            writer.WriteLine("[TimingPoints]");

            SampleControlPoint lastRelevantSamplePoint = null;
            DifficultyControlPoint lastRelevantDifficultyPoint = null;

            bool isOsuRuleset = beatmap.BeatmapInfo.RulesetID == 0;

            // iterate over hitobjects and pull out all required sample and difficulty changes
            extractDifficultyControlPoints(beatmap.HitObjects);
            extractSampleControlPoints(beatmap.HitObjects);

            // handle scroll speed, which is stored as "slider velocity" in legacy formats.
            // this is relevant for scrolling ruleset beatmaps.
            if (!isOsuRuleset)
            {
                foreach (var point in legacyControlPoints.EffectPoints)
                    legacyControlPoints.Add(point.Time, new DifficultyControlPoint { SliderVelocity = point.ScrollSpeed });
            }

            foreach (var group in legacyControlPoints.Groups)
            {
                var groupTimingPoint = group.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault();

                // If the group contains a timing control point, it needs to be output separately.
                if (groupTimingPoint != null)
                {
                    writer.Write(FormattableString.Invariant($"{groupTimingPoint.Time},"));
                    writer.Write(FormattableString.Invariant($"{groupTimingPoint.BeatLength},"));
                    outputControlPointAt(groupTimingPoint.Time, true);
                }

                // Output any remaining effects as secondary non-timing control point.
                var difficultyPoint = legacyControlPoints.DifficultyPointAt(group.Time);
                writer.Write(FormattableString.Invariant($"{group.Time},"));
                writer.Write(FormattableString.Invariant($"{-100 / difficultyPoint.SliderVelocity},"));
                outputControlPointAt(group.Time, false);
            }

            void outputControlPointAt(double time, bool isTimingPoint)
            {
                var samplePoint = legacyControlPoints.SamplePointAt(time);
                var effectPoint = legacyControlPoints.EffectPointAt(time);

                // Apply the control point to a hit sample to uncover legacy properties (e.g. suffix)
                HitSampleInfo tempHitSample = samplePoint.ApplyTo(new ConvertHitObjectParser.LegacyHitSampleInfo(string.Empty));

                // Convert effect flags to the legacy format
                LegacyEffectFlags effectFlags = LegacyEffectFlags.None;
                if (effectPoint.KiaiMode)
                    effectFlags |= LegacyEffectFlags.Kiai;
                if (effectPoint.OmitFirstBarLine)
                    effectFlags |= LegacyEffectFlags.OmitFirstBarLine;

                writer.Write(FormattableString.Invariant($"{(int)legacyControlPoints.TimingPointAt(time).TimeSignature},"));
                writer.Write(FormattableString.Invariant($"{(int)toLegacySampleBank(tempHitSample.Bank)},"));
                writer.Write(FormattableString.Invariant($"{toLegacyCustomSampleBank(tempHitSample)},"));
                writer.Write(FormattableString.Invariant($"{tempHitSample.Volume},"));
                writer.Write(FormattableString.Invariant($"{(isTimingPoint ? '1' : '0')},"));
                writer.Write(FormattableString.Invariant($"{(int)effectFlags}"));
                writer.WriteLine();
            }

            IEnumerable<DifficultyControlPoint> collectDifficultyControlPoints(IEnumerable<HitObject> hitObjects)
            {
                if (!isOsuRuleset)
                    yield break;

                foreach (var hitObject in hitObjects)
                {
                    yield return hitObject.DifficultyControlPoint;

                    foreach (var nested in collectDifficultyControlPoints(hitObject.NestedHitObjects))
                        yield return nested;
                }
            }

            void extractDifficultyControlPoints(IEnumerable<HitObject> hitObjects)
            {
                foreach (var hDifficultyPoint in collectDifficultyControlPoints(hitObjects).OrderBy(dp => dp.Time))
                {
                    if (!hDifficultyPoint.IsRedundant(lastRelevantDifficultyPoint))
                    {
                        legacyControlPoints.Add(hDifficultyPoint.Time, hDifficultyPoint);
                        lastRelevantDifficultyPoint = hDifficultyPoint;
                    }
                }
            }

            IEnumerable<SampleControlPoint> collectSampleControlPoints(IEnumerable<HitObject> hitObjects)
            {
                foreach (var hitObject in hitObjects)
                {
                    yield return hitObject.SampleControlPoint;

                    foreach (var nested in collectSampleControlPoints(hitObject.NestedHitObjects))
                        yield return nested;
                }
            }

            void extractSampleControlPoints(IEnumerable<HitObject> hitObject)
            {
                foreach (var hSamplePoint in collectSampleControlPoints(hitObject).OrderBy(sp => sp.Time))
                {
                    if (!hSamplePoint.IsRedundant(lastRelevantSamplePoint))
                    {
                        legacyControlPoints.Add(hSamplePoint.Time, hSamplePoint);
                        lastRelevantSamplePoint = hSamplePoint;
                    }
                }
            }
        }

        private void handleColours(TextWriter writer)
        {
            var colours = skin?.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value;

            if (colours == null || colours.Count == 0)
                return;

            writer.WriteLine("[Colours]");

            for (int i = 0; i < colours.Count; i++)
            {
                var comboColour = colours[i];

                writer.Write(FormattableString.Invariant($"Combo{i}: "));
                writer.Write(FormattableString.Invariant($"{(byte)(comboColour.R * byte.MaxValue)},"));
                writer.Write(FormattableString.Invariant($"{(byte)(comboColour.G * byte.MaxValue)},"));
                writer.Write(FormattableString.Invariant($"{(byte)(comboColour.B * byte.MaxValue)},"));
                writer.Write(FormattableString.Invariant($"{(byte)(comboColour.A * byte.MaxValue)}"));
                writer.WriteLine();
            }
        }

        private void handleHitObjects(TextWriter writer)
        {
            writer.WriteLine("[HitObjects]");

            if (beatmap.HitObjects.Count == 0)
                return;

            foreach (var h in beatmap.HitObjects)
                handleHitObject(writer, h);
        }

        private void handleHitObject(TextWriter writer, HitObject hitObject)
        {
            Vector2 position = new Vector2(256, 192);

            switch (beatmap.BeatmapInfo.RulesetID)
            {
                case 0:
                case 2:
                    position = ((IHasPosition)hitObject).Position;
                    break;

                case 3:
                    int totalColumns = (int)Math.Max(1, beatmap.Difficulty.CircleSize);
                    position.X = (int)Math.Ceiling(((IHasXPosition)hitObject).X * (512f / totalColumns));
                    break;
            }

            writer.Write(FormattableString.Invariant($"{position.X},"));
            writer.Write(FormattableString.Invariant($"{position.Y},"));
            writer.Write(FormattableString.Invariant($"{hitObject.StartTime},"));
            writer.Write(FormattableString.Invariant($"{(int)getObjectType(hitObject)},"));
            writer.Write(FormattableString.Invariant($"{(int)toLegacyHitSoundType(hitObject.Samples)},"));

            if (hitObject is IHasPath path)
            {
                addPathData(writer, path, position);
                writer.Write(getSampleBank(hitObject.Samples));
            }
            else
            {
                if (hitObject is IHasDuration)
                    addEndTimeData(writer, hitObject);

                writer.Write(getSampleBank(hitObject.Samples));
            }

            writer.WriteLine();
        }

        private LegacyHitObjectType getObjectType(HitObject hitObject)
        {
            LegacyHitObjectType type = 0;

            if (hitObject is IHasCombo combo)
            {
                type = (LegacyHitObjectType)(combo.ComboOffset << 4);

                if (combo.NewCombo)
                    type |= LegacyHitObjectType.NewCombo;
            }

            switch (hitObject)
            {
                case IHasPath _:
                    type |= LegacyHitObjectType.Slider;
                    break;

                case IHasDuration _:
                    if (beatmap.BeatmapInfo.RulesetID == 3)
                        type |= LegacyHitObjectType.Hold;
                    else
                        type |= LegacyHitObjectType.Spinner;
                    break;

                default:
                    type |= LegacyHitObjectType.Circle;
                    break;
            }

            return type;
        }

        private void addPathData(TextWriter writer, IHasPath pathData, Vector2 position)
        {
            PathType? lastType = null;

            for (int i = 0; i < pathData.Path.ControlPoints.Count; i++)
            {
                PathControlPoint point = pathData.Path.ControlPoints[i];

                if (point.Type != null)
                {
                    // We've reached a new (explicit) segment!

                    // Explicit segments have a new format in which the type is injected into the middle of the control point string.
                    // To preserve compatibility with osu-stable as much as possible, explicit segments with the same type are converted to use implicit segments by duplicating the control point.
                    // One exception are consecutive perfect curves, which aren't supported in osu!stable and can lead to decoding issues if encoded as implicit segments
                    bool needsExplicitSegment = point.Type != lastType || point.Type == PathType.PerfectCurve;

                    // Another exception to this is when the last two control points of the last segment were duplicated. This is not a scenario supported by osu!stable.
                    // Lazer does not add implicit segments for the last two control points of _any_ explicit segment, so an explicit segment is forced in order to maintain consistency with the decoder.
                    if (i > 1)
                    {
                        // We need to use the absolute control point position to determine equality, otherwise floating point issues may arise.
                        Vector2 p1 = position + pathData.Path.ControlPoints[i - 1].Position;
                        Vector2 p2 = position + pathData.Path.ControlPoints[i - 2].Position;

                        if ((int)p1.X == (int)p2.X && (int)p1.Y == (int)p2.Y)
                            needsExplicitSegment = true;
                    }

                    if (needsExplicitSegment)
                    {
                        switch (point.Type)
                        {
                            case PathType.Bezier:
                                writer.Write("B|");
                                break;

                            case PathType.Catmull:
                                writer.Write("C|");
                                break;

                            case PathType.PerfectCurve:
                                writer.Write("P|");
                                break;

                            case PathType.Linear:
                                writer.Write("L|");
                                break;
                        }

                        lastType = point.Type;
                    }
                    else
                    {
                        // New segment with the same type - duplicate the control point
                        writer.Write(FormattableString.Invariant($"{position.X + point.Position.X}:{position.Y + point.Position.Y}|"));
                    }
                }

                if (i != 0)
                {
                    writer.Write(FormattableString.Invariant($"{position.X + point.Position.X}:{position.Y + point.Position.Y}"));
                    writer.Write(i != pathData.Path.ControlPoints.Count - 1 ? "|" : ",");
                }
            }

            var curveData = pathData as IHasPathWithRepeats;

            writer.Write(FormattableString.Invariant($"{(curveData?.RepeatCount ?? 0) + 1},"));
            writer.Write(FormattableString.Invariant($"{pathData.Path.ExpectedDistance.Value ?? pathData.Path.Distance},"));

            if (curveData != null)
            {
                for (int i = 0; i < curveData.NodeSamples.Count; i++)
                {
                    writer.Write(FormattableString.Invariant($"{(int)toLegacyHitSoundType(curveData.NodeSamples[i])}"));
                    writer.Write(i != curveData.NodeSamples.Count - 1 ? "|" : ",");
                }

                for (int i = 0; i < curveData.NodeSamples.Count; i++)
                {
                    writer.Write(getSampleBank(curveData.NodeSamples[i], true));
                    writer.Write(i != curveData.NodeSamples.Count - 1 ? "|" : ",");
                }
            }
        }

        private void addEndTimeData(TextWriter writer, HitObject hitObject)
        {
            var endTimeData = (IHasDuration)hitObject;
            var type = getObjectType(hitObject);

            char suffix = ',';

            // Holds write the end time as if it's part of sample data.
            if (type == LegacyHitObjectType.Hold)
                suffix = ':';

            writer.Write(FormattableString.Invariant($"{endTimeData.EndTime}{suffix}"));
        }

        private string getSampleBank(IList<HitSampleInfo> samples, bool banksOnly = false)
        {
            LegacySampleBank normalBank = toLegacySampleBank(samples.SingleOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)?.Bank);
            LegacySampleBank addBank = toLegacySampleBank(samples.FirstOrDefault(s => !string.IsNullOrEmpty(s.Name) && s.Name != HitSampleInfo.HIT_NORMAL)?.Bank);

            StringBuilder sb = new StringBuilder();

            sb.Append(FormattableString.Invariant($"{(int)normalBank}:"));
            sb.Append(FormattableString.Invariant($"{(int)addBank}"));

            if (!banksOnly)
            {
                string customSampleBank = toLegacyCustomSampleBank(samples.FirstOrDefault(s => !string.IsNullOrEmpty(s.Name)));
                string sampleFilename = samples.FirstOrDefault(s => string.IsNullOrEmpty(s.Name))?.LookupNames.First() ?? string.Empty;
                int volume = samples.FirstOrDefault()?.Volume ?? 100;

                sb.Append(':');
                sb.Append(FormattableString.Invariant($"{customSampleBank}:"));
                sb.Append(FormattableString.Invariant($"{volume}:"));
                sb.Append(FormattableString.Invariant($"{sampleFilename}"));
            }

            return sb.ToString();
        }

        private LegacyHitSoundType toLegacyHitSoundType(IList<HitSampleInfo> samples)
        {
            LegacyHitSoundType type = LegacyHitSoundType.None;

            foreach (var sample in samples)
            {
                switch (sample.Name)
                {
                    case HitSampleInfo.HIT_WHISTLE:
                        type |= LegacyHitSoundType.Whistle;
                        break;

                    case HitSampleInfo.HIT_FINISH:
                        type |= LegacyHitSoundType.Finish;
                        break;

                    case HitSampleInfo.HIT_CLAP:
                        type |= LegacyHitSoundType.Clap;
                        break;
                }
            }

            return type;
        }

        private LegacySampleBank toLegacySampleBank(string sampleBank)
        {
            switch (sampleBank?.ToLowerInvariant())
            {
                case "normal":
                    return LegacySampleBank.Normal;

                case "soft":
                    return LegacySampleBank.Soft;

                case "drum":
                    return LegacySampleBank.Drum;

                default:
                    return LegacySampleBank.None;
            }
        }

        private string toLegacyCustomSampleBank(HitSampleInfo hitSampleInfo)
        {
            if (hitSampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacy)
                return legacy.CustomSampleBank.ToString(CultureInfo.InvariantCulture);

            return "0";
        }
    }
}
