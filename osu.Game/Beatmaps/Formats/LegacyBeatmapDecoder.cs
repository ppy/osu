// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.IO.File;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapDecoder : LegacyDecoder<Beatmap>
    {
        public const int LATEST_VERSION = 14;

        private ConvertHitObjectParser parser;

        private LegacySampleBank defaultSampleBank;
        private int defaultSampleVolume = 100;

        public static void Register()
        {
            AddDecoder<Beatmap>(@"osu file format v", m => new LegacyBeatmapDecoder(Parsing.ParseInt(m.Split('v').Last())));
            SetFallbackDecoder<Beatmap>(line =>
            {
                var decoder = new LegacyBeatmapDecoder();
                // it's possible that the first line was a section header - make sure it is parsed
                decoder.ParseLine(line);
                return decoder;
            });
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

        protected override void ParseStream(StreamReader stream)
        {
            Output.BeatmapInfo.BeatmapVersion = FormatVersion;

            base.ParseStream(stream);

            // Objects may be out of order *only* if a user has manually edited an .osu file.
            // Unfortunately there are ranked maps in this state (example: https://osu.ppy.sh/s/594828).
            // OrderBy is used to guarantee that the parsing order of hitobjects with equal start times is maintained (stably-sorted)
            // The parsing order of hitobjects matters in mania difficulty calculation
            Output.HitObjects = Output.HitObjects.OrderBy(h => h.StartTime).ToList();

            foreach (var hitObject in Output.HitObjects)
                hitObject.ApplyDefaults(Output.ControlPointInfo, Output.BeatmapInfo.BaseDifficulty);
        }

        protected override bool ShouldSkipLine(string line) => base.ShouldSkipLine(line) || line.StartsWith(" ", StringComparison.Ordinal) || line.StartsWith("_", StringComparison.Ordinal);

        protected override void ParseSectionLine(string line)
        {
            var strippedLine = StripComments(line);

            switch (ConfigSection)
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

            base.ParseSectionLine(line);
        }

        private void handleGeneral(string line)
        {
            var pair = SplitKeyVal(line);

            var metadata = Output.BeatmapInfo.Metadata;

            switch (pair.Key)
            {
                case @"AudioFilename":
                    metadata.AudioFile = FileSafety.PathStandardise(pair.Value);
                    break;

                case @"AudioLeadIn":
                    Output.BeatmapInfo.AudioLeadIn = Parsing.ParseInt(pair.Value);
                    break;

                case @"PreviewTime":
                    metadata.PreviewTime = getOffsetTime(Parsing.ParseInt(pair.Value));
                    break;

                case @"Countdown":
                    Output.BeatmapInfo.Countdown = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"SampleSet":
                    defaultSampleBank = (LegacySampleBank)Enum.Parse(typeof(LegacySampleBank), pair.Value);
                    break;

                case @"SampleVolume":
                    defaultSampleVolume = Parsing.ParseInt(pair.Value);
                    break;

                case @"StackLeniency":
                    Output.BeatmapInfo.StackLeniency = Parsing.ParseFloat(pair.Value);
                    break;

                case @"Mode":
                    Output.BeatmapInfo.RulesetID = Parsing.ParseInt(pair.Value);

                    switch (Output.BeatmapInfo.RulesetID)
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
                    Output.BeatmapInfo.LetterboxInBreaks = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"SpecialStyle":
                    Output.BeatmapInfo.SpecialStyle = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"WidescreenStoryboard":
                    Output.BeatmapInfo.WidescreenStoryboard = Parsing.ParseInt(pair.Value) == 1;
                    break;
            }
        }

        private void handleEditor(string line)
        {
            var pair = SplitKeyVal(line);

            switch (pair.Key)
            {
                case @"Bookmarks":
                    Output.BeatmapInfo.StoredBookmarks = pair.Value;
                    break;

                case @"DistanceSpacing":
                    Output.BeatmapInfo.DistanceSpacing = Math.Max(0, Parsing.ParseDouble(pair.Value));
                    break;

                case @"BeatDivisor":
                    Output.BeatmapInfo.BeatDivisor = Parsing.ParseInt(pair.Value);
                    break;

                case @"GridSize":
                    Output.BeatmapInfo.GridSize = Parsing.ParseInt(pair.Value);
                    break;

                case @"TimelineZoom":
                    Output.BeatmapInfo.TimelineZoom = Math.Max(0, Parsing.ParseDouble(pair.Value));
                    break;
            }
        }

        private void handleMetadata(string line)
        {
            var pair = SplitKeyVal(line);

            var metadata = Output.BeatmapInfo.Metadata;

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
                    Output.BeatmapInfo.Version = pair.Value;
                    break;

                case @"Source":
                    Output.BeatmapInfo.Metadata.Source = pair.Value;
                    break;

                case @"Tags":
                    Output.BeatmapInfo.Metadata.Tags = pair.Value;
                    break;

                case @"BeatmapID":
                    Output.BeatmapInfo.OnlineBeatmapID = Parsing.ParseInt(pair.Value);
                    break;

                case @"BeatmapSetID":
                    Output.BeatmapInfo.BeatmapSet = new BeatmapSetInfo { OnlineBeatmapSetID = Parsing.ParseInt(pair.Value) };
                    break;
            }
        }

        private void handleDifficulty(string line)
        {
            var pair = SplitKeyVal(line);

            var difficulty = Output.BeatmapInfo.BaseDifficulty;

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

            EventType type;

            if (!Enum.TryParse(split[0], out type))
                throw new InvalidDataException($@"Unknown event type: {split[0]}");

            switch (type)
            {
                case EventType.Background:
                    string filename = split[2].Trim('"');
                    Output.BeatmapInfo.Metadata.BackgroundFile = FileSafety.PathStandardise(filename);
                    break;

                case EventType.Break:
                    double start = getOffsetTime(Parsing.ParseDouble(split[1]));

                    var breakEvent = new BreakPeriod
                    {
                        StartTime = start,
                        EndTime = Math.Max(start, getOffsetTime(Parsing.ParseDouble(split[2])))
                    };

                    if (!breakEvent.HasEffect)
                        return;

                    Output.Breaks.Add(breakEvent);
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
                EffectFlags effectFlags = (EffectFlags)Parsing.ParseInt(split[7]);
                kiaiMode = effectFlags.HasFlag(EffectFlags.Kiai);
                omitFirstBarSignature = effectFlags.HasFlag(EffectFlags.OmitFirstBarLine);
            }

            string stringSampleSet = sampleSet.ToString().ToLowerInvariant();
            if (stringSampleSet == @"none")
                stringSampleSet = @"normal";

            if (timingChange)
            {
                var controlPoint = CreateTimingControlPoint();
                controlPoint.Time = time;
                controlPoint.BeatLength = beatLength;
                controlPoint.TimeSignature = timeSignature;

                handleTimingControlPoint(controlPoint);
            }

            handleDifficultyControlPoint(new DifficultyControlPoint
            {
                Time = time,
                SpeedMultiplier = speedMultiplier,
                AutoGenerated = timingChange
            });

            handleEffectControlPoint(new EffectControlPoint
            {
                Time = time,
                KiaiMode = kiaiMode,
                OmitFirstBarLine = omitFirstBarSignature,
                AutoGenerated = timingChange
            });

            handleSampleControlPoint(new LegacySampleControlPoint
            {
                Time = time,
                SampleBank = stringSampleSet,
                SampleVolume = sampleVolume,
                CustomSampleBank = customSampleBank,
                AutoGenerated = timingChange
            });
        }

        private void handleTimingControlPoint(TimingControlPoint newPoint)
        {
            var existing = Output.ControlPointInfo.TimingPointAt(newPoint.Time);

            if (existing.Time == newPoint.Time)
            {
                // autogenerated points should not replace non-autogenerated.
                // this allows for incorrectly ordered timing points to still be correctly handled.
                if (newPoint.AutoGenerated && !existing.AutoGenerated)
                    return;

                Output.ControlPointInfo.TimingPoints.Remove(existing);
            }

            Output.ControlPointInfo.TimingPoints.Add(newPoint);
        }

        private void handleDifficultyControlPoint(DifficultyControlPoint newPoint)
        {
            var existing = Output.ControlPointInfo.DifficultyPointAt(newPoint.Time);

            if (existing.Time == newPoint.Time)
            {
                // autogenerated points should not replace non-autogenerated.
                // this allows for incorrectly ordered timing points to still be correctly handled.
                if (newPoint.AutoGenerated && !existing.AutoGenerated)
                    return;

                Output.ControlPointInfo.DifficultyPoints.Remove(existing);
            }

            Output.ControlPointInfo.DifficultyPoints.Add(newPoint);
        }

        private void handleEffectControlPoint(EffectControlPoint newPoint)
        {
            var existing = Output.ControlPointInfo.EffectPointAt(newPoint.Time);

            if (existing.Time == newPoint.Time)
            {
                // autogenerated points should not replace non-autogenerated.
                // this allows for incorrectly ordered timing points to still be correctly handled.
                if (newPoint.AutoGenerated && !existing.AutoGenerated)
                    return;

                Output.ControlPointInfo.EffectPoints.Remove(existing);
            }

            Output.ControlPointInfo.EffectPoints.Add(newPoint);
        }

        private void handleSampleControlPoint(SampleControlPoint newPoint)
        {
            var existing = Output.ControlPointInfo.SamplePointAt(newPoint.Time);

            if (existing.Time == newPoint.Time)
            {
                // autogenerated points should not replace non-autogenerated.
                // this allows for incorrectly ordered timing points to still be correctly handled.
                if (newPoint.AutoGenerated && !existing.AutoGenerated)
                    return;

                Output.ControlPointInfo.SamplePoints.Remove(existing);
            }

            Output.ControlPointInfo.SamplePoints.Add(newPoint);
        }

        private void handleHitObject(string line)
        {
            // If the ruleset wasn't specified, assume the osu!standard ruleset.
            if (parser == null)
                parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser(getOffsetTime(), FormatVersion);

            var obj = parser.Parse(line);
            if (obj != null)
                Output.HitObjects.Add(obj);
        }

        private int getOffsetTime(int time) => time + (ApplyOffsets ? offset : 0);

        private double getOffsetTime() => ApplyOffsets ? offset : 0;

        private double getOffsetTime(double time) => time + (ApplyOffsets ? offset : 0);

        protected virtual TimingControlPoint CreateTimingControlPoint() => new TimingControlPoint();

        [Flags]
        internal enum EffectFlags
        {
            None = 0,
            Kiai = 1,
            OmitFirstBarLine = 8
        }
    }
}
