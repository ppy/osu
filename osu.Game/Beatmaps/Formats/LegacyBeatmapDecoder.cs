﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO;
using osu.Game.Beatmaps.Legacy;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapDecoder : LegacyDecoder<Beatmap>
    {
        public const int LATEST_VERSION = 14;

        private Beatmap beatmap;

        private ConvertHitObjectParser parser;

        private LegacySampleBank defaultSampleBank;
        private int defaultSampleVolume = 100;

        public static void Register()
        {
            AddDecoder<Beatmap>(@"osu file format v", m => new LegacyBeatmapDecoder(Parsing.ParseInt(m.Split('v').Last())));
            SetFallbackDecoder<Beatmap>(() => new LegacyBeatmapDecoder());
        }

        /// <summary>
        /// Whether or not beatmap or runtime offsets should be applied. Defaults on; only disable for testing purposes.
        /// </summary>
        public bool ApplyOffsets = true;

        private readonly int offset;

        public LegacyBeatmapDecoder(int version = LATEST_VERSION)
            : base(version)
        {
            // BeatmapVersion 4 and lower had an incorrect offset (stable has this set as 24ms off)
            offset = FormatVersion < 5 ? 24 : 0;
        }

        protected override void ParseStreamInto(LineBufferedReader stream, Beatmap beatmap)
        {
            this.beatmap = beatmap;
            this.beatmap.BeatmapInfo.BeatmapVersion = FormatVersion;

            base.ParseStreamInto(stream, beatmap);

            flushPendingPoints();

            // Objects may be out of order *only* if a user has manually edited an .osu file.
            // Unfortunately there are ranked maps in this state (example: https://osu.ppy.sh/s/594828).
            // OrderBy is used to guarantee that the parsing order of hitobjects with equal start times is maintained (stably-sorted)
            // The parsing order of hitobjects matters in mania difficulty calculation
            this.beatmap.HitObjects = this.beatmap.HitObjects.OrderBy(h => h.StartTime).ToList();

            foreach (var hitObject in this.beatmap.HitObjects)
                hitObject.ApplyDefaults(this.beatmap.ControlPointInfo, this.beatmap.BeatmapInfo.BaseDifficulty);
        }

        protected override bool ShouldSkipLine(string line) => base.ShouldSkipLine(line) || line.StartsWith(" ", StringComparison.Ordinal) || line.StartsWith("_", StringComparison.Ordinal);

        protected override void ParseLine(Beatmap beatmap, Section section, string line)
        {
            var strippedLine = StripComments(line);

            switch (section)
            {
                case Section.General:
                    handleGeneral(strippedLine);
                    return;

                case Section.Editor:
                    handleEditor(strippedLine);
                    return;

                case Section.Metadata:
                    handleMetadata(line);
                    return;

                case Section.Difficulty:
                    handleDifficulty(strippedLine);
                    return;

                case Section.Events:
                    handleEvent(strippedLine);
                    return;

                case Section.TimingPoints:
                    handleTimingPoint(strippedLine);
                    return;

                case Section.HitObjects:
                    handleHitObject(strippedLine);
                    return;
            }

            base.ParseLine(beatmap, section, line);
        }

        private void handleGeneral(string line)
        {
            var pair = SplitKeyVal(line);

            var metadata = beatmap.BeatmapInfo.Metadata;

            switch (pair.Key)
            {
                case @"AudioFilename":
                    metadata.AudioFile = pair.Value.ToStandardisedPath();
                    break;

                case @"AudioLeadIn":
                    beatmap.BeatmapInfo.AudioLeadIn = Parsing.ParseInt(pair.Value);
                    break;

                case @"PreviewTime":
                    metadata.PreviewTime = getOffsetTime(Parsing.ParseInt(pair.Value));
                    break;

                case @"Countdown":
                    beatmap.BeatmapInfo.Countdown = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"SampleSet":
                    defaultSampleBank = (LegacySampleBank)Enum.Parse(typeof(LegacySampleBank), pair.Value);
                    break;

                case @"SampleVolume":
                    defaultSampleVolume = Parsing.ParseInt(pair.Value);
                    break;

                case @"StackLeniency":
                    beatmap.BeatmapInfo.StackLeniency = Parsing.ParseFloat(pair.Value);
                    break;

                case @"Mode":
                    beatmap.BeatmapInfo.RulesetID = Parsing.ParseInt(pair.Value);

                    switch (beatmap.BeatmapInfo.RulesetID)
                    {
                        case 0:
                            parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;

                        case 1:
                            parser = new Rulesets.Objects.Legacy.Taiko.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;

                        case 2:
                            parser = new Rulesets.Objects.Legacy.Catch.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;

                        case 3:
                            parser = new Rulesets.Objects.Legacy.Mania.ConvertHitObjectParser(getOffsetTime(), FormatVersion);
                            break;
                    }

                    break;

                case @"LetterboxInBreaks":
                    beatmap.BeatmapInfo.LetterboxInBreaks = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"SpecialStyle":
                    beatmap.BeatmapInfo.SpecialStyle = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"WidescreenStoryboard":
                    beatmap.BeatmapInfo.WidescreenStoryboard = Parsing.ParseInt(pair.Value) == 1;
                    break;
            }
        }

        private void handleEditor(string line)
        {
            var pair = SplitKeyVal(line);

            switch (pair.Key)
            {
                case @"Bookmarks":
                    beatmap.BeatmapInfo.StoredBookmarks = pair.Value;
                    break;

                case @"DistanceSpacing":
                    beatmap.BeatmapInfo.DistanceSpacing = Math.Max(0, Parsing.ParseDouble(pair.Value));
                    break;

                case @"BeatDivisor":
                    beatmap.BeatmapInfo.BeatDivisor = Parsing.ParseInt(pair.Value);
                    break;

                case @"GridSize":
                    beatmap.BeatmapInfo.GridSize = Parsing.ParseInt(pair.Value);
                    break;

                case @"TimelineZoom":
                    beatmap.BeatmapInfo.TimelineZoom = Math.Max(0, Parsing.ParseDouble(pair.Value));
                    break;
            }
        }

        private void handleMetadata(string line)
        {
            var pair = SplitKeyVal(line);

            var metadata = beatmap.BeatmapInfo.Metadata;

            switch (pair.Key)
            {
                case @"Title":
                    metadata.Title = pair.Value;
                    break;

                case @"TitleUnicode":
                    metadata.TitleUnicode = pair.Value;
                    break;

                case @"Artist":
                    metadata.Artist = pair.Value;
                    break;

                case @"ArtistUnicode":
                    metadata.ArtistUnicode = pair.Value;
                    break;

                case @"Creator":
                    metadata.AuthorString = pair.Value;
                    break;

                case @"Version":
                    beatmap.BeatmapInfo.Version = pair.Value;
                    break;

                case @"Source":
                    beatmap.BeatmapInfo.Metadata.Source = pair.Value;
                    break;

                case @"Tags":
                    beatmap.BeatmapInfo.Metadata.Tags = pair.Value;
                    break;

                case @"BeatmapID":
                    beatmap.BeatmapInfo.OnlineBeatmapID = Parsing.ParseInt(pair.Value);
                    break;

                case @"BeatmapSetID":
                    beatmap.BeatmapInfo.BeatmapSet = new BeatmapSetInfo { OnlineBeatmapSetID = Parsing.ParseInt(pair.Value) };
                    break;
            }
        }

        private void handleDifficulty(string line)
        {
            var pair = SplitKeyVal(line);

            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;

            switch (pair.Key)
            {
                case @"HPDrainRate":
                    difficulty.DrainRate = Parsing.ParseFloat(pair.Value);
                    break;

                case @"CircleSize":
                    difficulty.CircleSize = Parsing.ParseFloat(pair.Value);
                    break;

                case @"OverallDifficulty":
                    difficulty.OverallDifficulty = Parsing.ParseFloat(pair.Value);
                    break;

                case @"ApproachRate":
                    difficulty.ApproachRate = Parsing.ParseFloat(pair.Value);
                    break;

                case @"SliderMultiplier":
                    difficulty.SliderMultiplier = Parsing.ParseDouble(pair.Value);
                    break;

                case @"SliderTickRate":
                    difficulty.SliderTickRate = Parsing.ParseDouble(pair.Value);
                    break;
            }
        }

        private void handleEvent(string line)
        {
            string[] split = line.Split(',');

            if (!Enum.TryParse(split[0], out LegacyEventType type))
                throw new InvalidDataException($@"Unknown event type: {split[0]}");

            switch (type)
            {
                case LegacyEventType.Background:
                    string bgFilename = split[2].Trim('"');
                    beatmap.BeatmapInfo.Metadata.BackgroundFile = bgFilename.ToStandardisedPath();
                    break;

                case LegacyEventType.Video:
                    string videoFilename = split[2].Trim('"');
                    beatmap.BeatmapInfo.Metadata.VideoFile = videoFilename.ToStandardisedPath();
                    break;

                case LegacyEventType.Break:
                    double start = getOffsetTime(Parsing.ParseDouble(split[1]));

                    var breakEvent = new BreakPeriod
                    {
                        StartTime = start,
                        EndTime = Math.Max(start, getOffsetTime(Parsing.ParseDouble(split[2])))
                    };

                    if (!breakEvent.HasEffect)
                        return;

                    beatmap.Breaks.Add(breakEvent);
                    break;
            }
        }

        private void handleTimingPoint(string line)
        {
            string[] split = line.Split(',');

            double time = getOffsetTime(Parsing.ParseDouble(split[0].Trim()));
            double beatLength = Parsing.ParseDouble(split[1].Trim());
            double speedMultiplier = beatLength < 0 ? 100.0 / -beatLength : 1;

            TimeSignatures timeSignature = TimeSignatures.SimpleQuadruple;
            if (split.Length >= 3)
                timeSignature = split[2][0] == '0' ? TimeSignatures.SimpleQuadruple : (TimeSignatures)Parsing.ParseInt(split[2]);

            LegacySampleBank sampleSet = defaultSampleBank;
            if (split.Length >= 4)
                sampleSet = (LegacySampleBank)Parsing.ParseInt(split[3]);

            int customSampleBank = 0;
            if (split.Length >= 5)
                customSampleBank = Parsing.ParseInt(split[4]);

            int sampleVolume = defaultSampleVolume;
            if (split.Length >= 6)
                sampleVolume = Parsing.ParseInt(split[5]);

            bool timingChange = true;
            if (split.Length >= 7)
                timingChange = split[6][0] == '1';

            bool kiaiMode = false;
            bool omitFirstBarSignature = false;

            if (split.Length >= 8)
            {
                LegacyEffectFlags effectFlags = (LegacyEffectFlags)Parsing.ParseInt(split[7]);
                kiaiMode = effectFlags.HasFlag(LegacyEffectFlags.Kiai);
                omitFirstBarSignature = effectFlags.HasFlag(LegacyEffectFlags.OmitFirstBarLine);
            }

            string stringSampleSet = sampleSet.ToString().ToLowerInvariant();
            if (stringSampleSet == @"none")
                stringSampleSet = @"normal";

            if (timingChange)
            {
                var controlPoint = CreateTimingControlPoint();

                controlPoint.BeatLength = beatLength;
                controlPoint.TimeSignature = timeSignature;

                addControlPoint(time, controlPoint, true);
            }

            addControlPoint(time, new LegacyDifficultyControlPoint
            {
                SpeedMultiplier = speedMultiplier,
            }, timingChange);

            addControlPoint(time, new EffectControlPoint
            {
                KiaiMode = kiaiMode,
                OmitFirstBarLine = omitFirstBarSignature,
            }, timingChange);

            addControlPoint(time, new LegacySampleControlPoint
            {
                SampleBank = stringSampleSet,
                SampleVolume = sampleVolume,
                CustomSampleBank = customSampleBank,
            }, timingChange);

            // To handle the scenario where a non-timing line shares the same time value as a subsequent timing line but
            // appears earlier in the file, we buffer non-timing control points and rewrite them *after* control points from the timing line
            // with the same time value (allowing them to overwrite as necessary).
            //
            // The expected outcome is that we prefer the non-timing line's adjustments over the timing line's adjustments when time is equal.
            if (timingChange)
                flushPendingPoints();
        }

        private readonly List<ControlPoint> pendingControlPoints = new List<ControlPoint>();
        private double pendingControlPointsTime;

        private void addControlPoint(double time, ControlPoint point, bool timingChange)
        {
            if (time != pendingControlPointsTime)
                flushPendingPoints();

            if (timingChange)
            {
                beatmap.ControlPointInfo.Add(time, point);
                return;
            }

            pendingControlPoints.Add(point);
            pendingControlPointsTime = time;
        }

        private void flushPendingPoints()
        {
            foreach (var p in pendingControlPoints)
                beatmap.ControlPointInfo.Add(pendingControlPointsTime, p);

            pendingControlPoints.Clear();
        }

        private void handleHitObject(string line)
        {
            // If the ruleset wasn't specified, assume the osu!standard ruleset.
            if (parser == null)
                parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser(getOffsetTime(), FormatVersion);

            var obj = parser.Parse(line);
            if (obj != null)
                beatmap.HitObjects.Add(obj);
        }

        private int getOffsetTime(int time) => time + (ApplyOffsets ? offset : 0);

        private double getOffsetTime() => ApplyOffsets ? offset : 0;

        private double getOffsetTime(double time) => time + (ApplyOffsets ? offset : 0);

        protected virtual TimingControlPoint CreateTimingControlPoint() => new TimingControlPoint();
    }
}
