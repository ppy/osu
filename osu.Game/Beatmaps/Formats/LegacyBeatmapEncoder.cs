// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapEncoder
    {
        public const int LATEST_VERSION = 128;

        private readonly IBeatmap beatmap;

        public LegacyBeatmapEncoder(IBeatmap beatmap)
        {
            this.beatmap = beatmap;

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
            handleTimingPoints(writer);

            writer.WriteLine();
            handleHitObjects(writer);
        }

        private void handleGeneral(TextWriter writer)
        {
            writer.WriteLine("[General]");

            writer.WriteLine(FormattableString.Invariant($"AudioFilename: {Path.GetFileName(beatmap.Metadata.AudioFile)}"));
            writer.WriteLine(FormattableString.Invariant($"AudioLeadIn: {beatmap.BeatmapInfo.AudioLeadIn}"));
            writer.WriteLine(FormattableString.Invariant($"PreviewTime: {beatmap.Metadata.PreviewTime}"));
            // Todo: Not all countdown types are supported by lazer yet
            writer.WriteLine(FormattableString.Invariant($"Countdown: {(beatmap.BeatmapInfo.Countdown ? '1' : '0')}"));
            writer.WriteLine(FormattableString.Invariant($"SampleSet: {toLegacySampleBank(beatmap.ControlPointInfo.SamplePointAt(double.MinValue).SampleBank)}"));
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
            // if (b.EpilepsyWarning)
            //     writer.WriteLine(@"EpilepsyWarning: 1");
            // if (b.CountdownOffset > 0)
            //     writer.WriteLine(@"CountdownOffset: " + b.CountdownOffset.ToString());
            if (beatmap.BeatmapInfo.RulesetID == 3)
                writer.WriteLine(FormattableString.Invariant($"SpecialStyle: {(beatmap.BeatmapInfo.SpecialStyle ? '1' : '0')}"));
            writer.WriteLine(FormattableString.Invariant($"WidescreenStoryboard: {(beatmap.BeatmapInfo.WidescreenStoryboard ? '1' : '0')}"));
            // if (b.SamplesMatchPlaybackRate)
            //     writer.WriteLine(@"SamplesMatchPlaybackRate: 1");
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
            writer.WriteLine(FormattableString.Invariant($"TitleUnicode: {beatmap.Metadata.TitleUnicode}"));
            writer.WriteLine(FormattableString.Invariant($"Artist: {beatmap.Metadata.Artist}"));
            writer.WriteLine(FormattableString.Invariant($"ArtistUnicode: {beatmap.Metadata.ArtistUnicode}"));
            writer.WriteLine(FormattableString.Invariant($"Creator: {beatmap.Metadata.AuthorString}"));
            writer.WriteLine(FormattableString.Invariant($"Version: {beatmap.BeatmapInfo.Version}"));
            writer.WriteLine(FormattableString.Invariant($"Source: {beatmap.Metadata.Source}"));
            writer.WriteLine(FormattableString.Invariant($"Tags: {beatmap.Metadata.Tags}"));
            writer.WriteLine(FormattableString.Invariant($"BeatmapID: {beatmap.BeatmapInfo.OnlineBeatmapID ?? 0}"));
            writer.WriteLine(FormattableString.Invariant($"BeatmapSetID: {beatmap.BeatmapInfo.BeatmapSet.OnlineBeatmapSetID ?? -1}"));
        }

        private void handleDifficulty(TextWriter writer)
        {
            writer.WriteLine("[Difficulty]");

            writer.WriteLine(FormattableString.Invariant($"HPDrainRate: {beatmap.BeatmapInfo.BaseDifficulty.DrainRate}"));
            writer.WriteLine(FormattableString.Invariant($"CircleSize: {beatmap.BeatmapInfo.BaseDifficulty.CircleSize}"));
            writer.WriteLine(FormattableString.Invariant($"OverallDifficulty: {beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty}"));
            writer.WriteLine(FormattableString.Invariant($"ApproachRate: {beatmap.BeatmapInfo.BaseDifficulty.ApproachRate}"));
            writer.WriteLine(FormattableString.Invariant($"SliderMultiplier: {beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier}"));
            writer.WriteLine(FormattableString.Invariant($"SliderTickRate: {beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate}"));
        }

        private void handleEvents(TextWriter writer)
        {
            writer.WriteLine("[Events]");

            if (!string.IsNullOrEmpty(beatmap.BeatmapInfo.Metadata.BackgroundFile))
                writer.WriteLine(FormattableString.Invariant($"{(int)LegacyEventType.Background},0,\"{beatmap.BeatmapInfo.Metadata.BackgroundFile}\",0,0"));

            if (!string.IsNullOrEmpty(beatmap.BeatmapInfo.Metadata.VideoFile))
                writer.WriteLine(FormattableString.Invariant($"{(int)LegacyEventType.Video},0,\"{beatmap.BeatmapInfo.Metadata.VideoFile}\",0,0"));

            foreach (var b in beatmap.Breaks)
                writer.WriteLine(FormattableString.Invariant($"{(int)LegacyEventType.Break},{b.StartTime},{b.EndTime}"));
        }

        private void handleTimingPoints(TextWriter writer)
        {
            if (beatmap.ControlPointInfo.Groups.Count == 0)
                return;

            writer.WriteLine("[TimingPoints]");

            foreach (var group in beatmap.ControlPointInfo.Groups)
            {
                var timingPoint = group.ControlPoints.OfType<TimingControlPoint>().FirstOrDefault();
                var difficultyPoint = beatmap.ControlPointInfo.DifficultyPointAt(group.Time);
                var samplePoint = beatmap.ControlPointInfo.SamplePointAt(group.Time);
                var effectPoint = beatmap.ControlPointInfo.EffectPointAt(group.Time);

                // Convert beat length the legacy format
                double beatLength;
                if (timingPoint != null)
                    beatLength = timingPoint.BeatLength;
                else
                    beatLength = -100 / difficultyPoint.SpeedMultiplier;

                // Apply the control point to a hit sample to uncover legacy properties (e.g. suffix)
                HitSampleInfo tempHitSample = samplePoint.ApplyTo(new HitSampleInfo());

                // Convert effect flags to the legacy format
                LegacyEffectFlags effectFlags = LegacyEffectFlags.None;
                if (effectPoint.KiaiMode)
                    effectFlags |= LegacyEffectFlags.Kiai;
                if (effectPoint.OmitFirstBarLine)
                    effectFlags |= LegacyEffectFlags.OmitFirstBarLine;

                writer.Write(FormattableString.Invariant($"{group.Time},"));
                writer.Write(FormattableString.Invariant($"{beatLength},"));
                writer.Write(FormattableString.Invariant($"{(int)beatmap.ControlPointInfo.TimingPointAt(group.Time).TimeSignature},"));
                writer.Write(FormattableString.Invariant($"{(int)toLegacySampleBank(tempHitSample.Bank)},"));
                writer.Write(FormattableString.Invariant($"{toLegacyCustomSampleBank(tempHitSample.Suffix)},"));
                writer.Write(FormattableString.Invariant($"{tempHitSample.Volume},"));
                writer.Write(FormattableString.Invariant($"{(timingPoint != null ? '1' : '0')},"));
                writer.Write(FormattableString.Invariant($"{(int)effectFlags}"));
                writer.WriteLine();
            }
        }

        private void handleHitObjects(TextWriter writer)
        {
            if (beatmap.HitObjects.Count == 0)
                return;

            writer.WriteLine("[HitObjects]");

            switch (beatmap.BeatmapInfo.RulesetID)
            {
                case 0:
                    foreach (var h in beatmap.HitObjects)
                        handleOsuHitObject(writer, h);
                    break;

                case 1:
                    foreach (var h in beatmap.HitObjects)
                        handleTaikoHitObject(writer, h);
                    break;

                case 2:
                    foreach (var h in beatmap.HitObjects)
                        handleCatchHitObject(writer, h);
                    break;

                case 3:
                    foreach (var h in beatmap.HitObjects)
                        handleManiaHitObject(writer, h);
                    break;
            }
        }

        private void handleOsuHitObject(TextWriter writer, HitObject hitObject)
        {
            var positionData = (IHasPosition)hitObject;

            writer.Write(FormattableString.Invariant($"{positionData.X},"));
            writer.Write(FormattableString.Invariant($"{positionData.Y},"));
            writer.Write(FormattableString.Invariant($"{hitObject.StartTime},"));
            writer.Write(FormattableString.Invariant($"{(int)getObjectType(hitObject)},"));

            writer.Write(hitObject is IHasCurve
                ? FormattableString.Invariant($"0,")
                : FormattableString.Invariant($"{(int)toLegacyHitSoundType(hitObject.Samples)},"));

            if (hitObject is IHasCurve curveData)
            {
                addCurveData(writer, curveData, positionData);
                writer.Write(getSampleBank(hitObject.Samples, zeroBanks: true));
            }
            else
            {
                if (hitObject is IHasEndTime endTimeData)
                    writer.Write(FormattableString.Invariant($"{endTimeData.EndTime},"));
                writer.Write(getSampleBank(hitObject.Samples));
            }

            writer.WriteLine();
        }

        private static LegacyHitObjectType getObjectType(HitObject hitObject)
        {
            var comboData = (IHasCombo)hitObject;

            var type = (LegacyHitObjectType)(comboData.ComboOffset << 4);

            if (comboData.NewCombo) type |= LegacyHitObjectType.NewCombo;

            switch (hitObject)
            {
                case IHasCurve _:
                    type |= LegacyHitObjectType.Slider;
                    break;

                case IHasEndTime _:
                    type |= LegacyHitObjectType.Spinner | LegacyHitObjectType.NewCombo;
                    break;

                default:
                    type |= LegacyHitObjectType.Circle;
                    break;
            }

            return type;
        }

        private void addCurveData(TextWriter writer, IHasCurve curveData, IHasPosition positionData)
        {
            PathType? lastType = null;

            for (int i = 0; i < curveData.Path.ControlPoints.Count; i++)
            {
                PathControlPoint point = curveData.Path.ControlPoints[i];

                if (point.Type.Value != null)
                {
                    if (point.Type.Value != lastType)
                    {
                        switch (point.Type.Value)
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

                        lastType = point.Type.Value;
                    }
                    else
                    {
                        // New segment with the same type - duplicate the control point
                        writer.Write(FormattableString.Invariant($"{positionData.X + point.Position.Value.X}:{positionData.Y + point.Position.Value.Y}|"));
                    }
                }

                if (i != 0)
                {
                    writer.Write(FormattableString.Invariant($"{positionData.X + point.Position.Value.X}:{positionData.Y + point.Position.Value.Y}"));
                    writer.Write(i != curveData.Path.ControlPoints.Count - 1 ? "|" : ",");
                }
            }

            writer.Write(FormattableString.Invariant($"{curveData.RepeatCount + 1},"));
            writer.Write(FormattableString.Invariant($"{curveData.Path.Distance},"));

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

        private void handleTaikoHitObject(TextWriter writer, HitObject hitObject) => throw new NotImplementedException();

        private void handleCatchHitObject(TextWriter writer, HitObject hitObject) => throw new NotImplementedException();

        private void handleManiaHitObject(TextWriter writer, HitObject hitObject) => throw new NotImplementedException();

        private string getSampleBank(IList<HitSampleInfo> samples, bool banksOnly = false, bool zeroBanks = false)
        {
            LegacySampleBank normalBank = toLegacySampleBank(samples.SingleOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)?.Bank);
            LegacySampleBank addBank = toLegacySampleBank(samples.FirstOrDefault(s => !string.IsNullOrEmpty(s.Name) && s.Name != HitSampleInfo.HIT_NORMAL)?.Bank);

            StringBuilder sb = new StringBuilder();

            sb.Append(FormattableString.Invariant($"{(zeroBanks ? 0 : (int)normalBank)}:"));
            sb.Append(FormattableString.Invariant($"{(zeroBanks ? 0 : (int)addBank)}"));

            if (!banksOnly)
            {
                string customSampleBank = toLegacyCustomSampleBank(samples.FirstOrDefault(s => !string.IsNullOrEmpty(s.Name))?.Suffix);
                string sampleFilename = samples.FirstOrDefault(s => string.IsNullOrEmpty(s.Name))?.LookupNames.First() ?? string.Empty;
                int volume = samples.FirstOrDefault()?.Volume ?? 100;

                sb.Append(":");
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

        private string toLegacyCustomSampleBank(string sampleSuffix) => string.IsNullOrEmpty(sampleSuffix) ? "0" : sampleSuffix;
    }
}
